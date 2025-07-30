using Microsoft.EntityFrameworkCore;
using SAM.T.Protocol;
using SAM.T.Protocol.Models;
using SAM.T.Worker.Data;
using SAM.T.Worker.Data.Models;
using System.Diagnostics;

namespace SAM.T.Worker.Services;

public class MonitoringService
{
    private readonly MonitoringContext _context;
    private readonly HttpClientService _httpClientService;
    private readonly AnalyticsService _analyticsService;

    public MonitoringService(MonitoringContext dataContext, HttpClientService httpClientService, AnalyticsService analyticsService)
    {
        _context = dataContext;
        _httpClientService = httpClientService;
        _analyticsService = analyticsService;
    }

    public async Task Execute()
    {
        var appMonitorings = await _context.MonitoredApplications.ToListAsync();

        if (!appMonitorings.Any())
            return;

        var monitoringTasks = new List<Task<MonitoringResult>>();

        foreach (var appMonitoring in appMonitorings)
            monitoringTasks.Add(RequestAndGetResult(appMonitoring));

        await Task.WhenAll(monitoringTasks);

        foreach (var task in monitoringTasks)
            _context.MonitoringResults.Add(task.Result);

        await _context.SaveChangesAsync();

        await _analyticsService.Analyse();
    }

    public async Task Execute(int monitoredApplicationId)
    {
        var app = await _context.MonitoredApplications.FindAsync(monitoredApplicationId);

        if (app == null)
            throw new ArgumentException($"Monitored application with ID {monitoredApplicationId} not found.");

        var result = await RequestAndGetResult(app);
        _context.MonitoringResults.Add(result);

        await _context.SaveChangesAsync();

        await _analyticsService.Analyse();
    }

    public async Task<MonitoringState[]> GetMonitoringStates()
    {
        return (await _context.MonitoringResults
            .Include(mr => mr.MonitoredApplication)
            .GroupBy(mr => mr.MonitoredApplicationId)
            .Select(gr => gr.OrderByDescending(g => g.Time).First())
            .ToListAsync())
            .Select(ToMonitoringState)
            .ToArray();
    }

    public async Task<MonitoredApplicationInfo> GetMonitoredApplicationInfo(int appId)
    {
        var monitoringResult = await _context.MonitoringResults
            .Include(mr => mr.MonitoredApplication)
            .Where(mr => mr.MonitoredApplicationId == appId)
            .OrderByDescending(g => g.Time)
            .FirstOrDefaultAsync() ?? throw new ArgumentException($"No monitoring result for application {appId} found.");

        var state = ToMonitoringState(monitoringResult);

        var monitoringResults = await _context.MonitoringResults
            .Include(mr => mr.Inner)
            .Where(mr => mr.MonitoredApplicationId == appId)
            .OrderByDescending(mr => mr.Time)
            .Select(mr => mr.Inner)
            .FirstOrDefaultAsync();

        List<MonitoringDetails> details = [];

        //TODO Optimize
        foreach (var result in monitoringResults ?? [])
        {
            var lastStatusChange = GetLastStatusChange(appId, result.Feature, result.State);

            details.Add(new MonitoringDetails
            {
                Feature = result.Feature,
                State = result.State,
                Message = result.Message,
                LastStatusChange = lastStatusChange?.time ?? null,
                LastStatusState = lastStatusChange?.state ?? result.State,
            });
        }

        return new MonitoredApplicationInfo { ApplicationState = state, Details = details.ToArray() };
    }

    private (DateTime time, HealthState state)? GetLastStatusChange(int appId, string feature, HealthState healthState)
    {
        var record = _context.HealthCheckRecords
            .Include(hcr => hcr.MonitoringResult)
            .Where(hcr => hcr.MonitoringResult.MonitoredApplicationId == appId && hcr.Feature == feature && hcr.State != healthState)
            .OrderByDescending(hcr => hcr.MonitoringResult.Time)
            .FirstOrDefault();

        if (record is null)
            return null;

        return (record.MonitoringResult.Time, record.State);
    }

    private async Task<MonitoringResult> RequestAndGetResult(MonitoredApplication app)
    {
        HttpRequestMessage req = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(app.Endpoint)
        };

        var stopWatch = Stopwatch.StartNew();

        HttpResponseMessage response;

        try
        {
            var client = _httpClientService.Get(app);
            response = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return FromException(ex, app);
        }

        stopWatch.Stop();

        var result = new MonitoringResult
        {
            State = response.IsSuccessStatusCode ? HealthState.Operational : HealthState.Error,
            MonitoredApplicationId = app.Id,
            Time = DateTime.UtcNow,
            ResponseCode = (int)response.StatusCode,
            ResponseTime = stopWatch.ElapsedMilliseconds
        };

        await ExtractHealthChecks(response, result);

        return result;
    }

    private static async Task ExtractHealthChecks(HttpResponseMessage response, MonitoringResult result)
    {
        // If healthcheck is found in body, add it to the record
        try
        {
            var healthCheck = await response.Content.ReadFromJsonAsync<HealthCheckResult>();

            if (healthCheck is not null)
            {
                result.State = healthCheck.State;
                result.Message = healthCheck.Message;

                result.Inner = healthCheck!.Inner?.Select(i => new HealthCheckRecord
                {
                    Feature = i.Feature,
                    State = i.State,
                    Message = i.Message,
                    MonitoringResult = result
                }).ToList() ?? [];
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }

    private static MonitoringResult FromException(Exception ex, MonitoredApplication app)
    {
        return new MonitoringResult
        {
            MonitoredApplicationId = app.Id,
            State = HealthState.Error,
            Fail = ex.Message,
            Time = DateTime.UtcNow
        };
    }

    private static MonitoringState ToMonitoringState(MonitoringResult mr)
    {
        return new MonitoringState
        {
            ApplicationId = mr.MonitoredApplicationId,
            ApplicationName = mr.MonitoredApplication.Name,
            ApplicationEnvironment = mr.MonitoredApplication.Environment,
            ApplicationUrl = mr.MonitoredApplication.Url,
            State = mr.State,
            Fail = mr.Fail,
            ResponseTime = mr.ResponseTime,
            ResponseTimeDeviation = GetResponseTimeState(mr.ResponseTimeDeviation),
            Message = mr.Message,
            LastUpdate = mr.Time,
        };
    }

    private static ResponseTimeState GetResponseTimeState(double responseTimeDeviation)
    {
        if (Math.Abs(responseTimeDeviation) < 1.5d)
            return ResponseTimeState.Average;

        if (responseTimeDeviation > 0)
        {
            if (responseTimeDeviation < 3)
                return ResponseTimeState.Slow;

            return ResponseTimeState.AbnormallySlow;
        }
        else
        {
            if (responseTimeDeviation > -3)
                return ResponseTimeState.Fast;

            return ResponseTimeState.BlazinglyFast;
        }
    }

    public async Task<List<ApplicationAvailability>> GetAvailability(int appId, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;

        var allResults = await _context.MonitoringResults
            .Where(mr => mr.MonitoredApplicationId == appId && mr.Time >= startDate)
            .ToListAsync();

        return allResults
            .GroupBy(mr => mr.Time.Date)
            .Select(g => new ApplicationAvailability { Date = g.Key, Availability = CalculateAvailability(g.ToList()) })
            .ToList();
    }

    private static float CalculateAvailability(List<MonitoringResult> monitoringResults)
    {
        return (float)monitoringResults.Count(mr => mr.State == HealthState.Operational) / monitoringResults.Count;
    }
}
