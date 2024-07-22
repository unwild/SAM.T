using Microsoft.EntityFrameworkCore;
using SAM.T.Protocol;
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

    public async Task<MonitoringState[]> GetRecords()
    {
        return (await _context.MonitoringResults
            .Include(mr => mr.MonitoredApplication)
            .GroupBy(mr => mr.MonitoredApplicationId)
            .Select(gr => gr.OrderByDescending(g => g.Time).First())
            .ToListAsync())
            .Select(mr => ToMonitoringState(mr)).ToArray();
    }

    private async Task<MonitoringResult> RequestAndGetResult(MonitoredApplication app)
    {
        HttpRequestMessage req = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(app.Url)
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
            ApplicationName = mr.MonitoredApplication.Name,
            State = mr.State,
            Fail = mr.Message,
            ResponseTimeDeviation = GetResponseTimeState(mr.ResponseTimeDeviation),
            Message = mr.Message
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
}
