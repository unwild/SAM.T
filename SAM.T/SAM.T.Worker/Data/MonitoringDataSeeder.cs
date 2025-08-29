
using Microsoft.EntityFrameworkCore;
using SAM.T.Protocol;
using SAM.T.Worker.Data.Models;

namespace SAM.T.Worker.Data;

public class MonitoringDataSeeder(MonitoringContext context)
{
    private const double ResponseTimeDeviationRange = 0.3;

    private readonly MonitoringContext _context = context;
    private readonly Random _random = new();

    private static readonly string[] Environments = ["Prod", "Preprod", "Staging", "Dev"];
    private static readonly string[] Features = ["Database", "S3", "Auth API", "Config File", "Third Party API"];
    
    // Tag categories for generating sample tags
    private static readonly string[] TagCategories = ["Team", "Version", "Region", "CriticalityLevel", "BusinessUnit"];
    private static readonly Dictionary<string, string[]> TagValues = new()
    {
        ["Team"] = ["Backend", "Frontend", "DevOps", "Data", "Security"],
        ["Version"] = ["v1.0", "v1.1", "v1.2", "v2.0", "v2.1"],
        ["Region"] = ["US-East", "US-West", "EU-Central", "Asia-Pacific"],
        ["CriticalityLevel"] = ["Critical", "High", "Medium", "Low"],
        ["BusinessUnit"] = ["Sales", "Marketing", "Support", "Analytics", "Core"]
    };

    private record AppThreshold(double DegradedThreshold, double ErrorThreshold);

    private Dictionary<int, AppThreshold> appThresholds = [];

    public async Task SeedAsync()
    {
        if (await _context.MonitoredApplications.AnyAsync())
            return;

        var applications = GenerateApplications(15);

        _context.MonitoredApplications.AddRange(applications);
        await _context.SaveChangesAsync();

        // Assign random thresholds for each application
        foreach (var app in applications)
        {
            appThresholds.Add(app.Id, new AppThreshold(
                DegradedThreshold: _random.NextDouble() * (20f / 100) + (5f / 100), // between 5% and 25%
                ErrorThreshold: _random.NextDouble() * (10f / 100) // between 0% and 10%
            ));
        }

        foreach (var app in applications)
            await GenerateAndInsertMonitoringDataAsync(app);
    }

    private List<MonitoredApplication> GenerateApplications(int count)
    {
        return Enumerable.Range(1, count).Select(i => new MonitoredApplication
        {
            Name = $"App_{i:D2}",
            Environment = Environments[_random.Next(Environments.Length)],
            Url = $"https://app{i:D2}.example.com",
            Endpoint = "/health",
            UseProxy = false,
            Tags = GenerateRandomTags()
        }).ToList();
    }

    private List<ApplicationTag> GenerateRandomTags()
    {
        var tags = new List<ApplicationTag>();
        
        // Generate 2-4 random tags per application
        var tagCount = _random.Next(2, 5);
        var selectedCategories = TagCategories.OrderBy(_ => _random.Next()).Take(tagCount).ToList();
        
        foreach (var category in selectedCategories)
        {
            var possibleValues = TagValues[category];
            var value = possibleValues[_random.Next(possibleValues.Length)];
            
            tags.Add(new ApplicationTag
            {
                Key = category,
                Value = value
            });
        }
        
        return tags;
    }

    private async Task GenerateAndInsertMonitoringDataAsync(MonitoredApplication app)
    {
        var start = DateTime.Now.AddMonths(-1);
        var end = DateTime.Now;

        var batchSize = 100;
        var results = new List<MonitoringResult>();

        var current = start;
        while (current <= end)
        {
            var result = GenerateMonitoringResult(app, current);
            results.Add(result);

            if (results.Count >= batchSize)
            {
                await InsertBatchAsync(results);
                results.Clear();
            }

            current = current.AddMinutes(30);
        }

        if (results.Count > 0)
            await InsertBatchAsync(results);
    }

    private MonitoringResult GenerateMonitoringResult(MonitoredApplication app, DateTime timestamp)
    {
        var responseTime = _random.Next(50, 800);
        var responseCode = 200;

        var result = new MonitoringResult
        {
            Time = timestamp,
            ResponseCode = responseCode,
            State = HealthState.Operational, // Temporary, will be updated later
            Message = "",
            Fail = null,
            ResponseTime = responseTime,
            ResponseTimeDeviation = _random.NextDouble() * ResponseTimeDeviationRange,
            MonitoredApplicationId = app.Id,
            Inner = []
        };

        // Generate random subset of features
        var selectedFeatures = Features.OrderBy(_ => _random.Next()).Take(_random.Next(2, Features.Length)).ToList();
        var featureStates = new List<HealthState>();

        foreach (var feature in selectedFeatures)
        {
            // Independent roll for each feature
            var roll = _random.NextDouble();
            HealthState featureState;

            if (roll < appThresholds[app.Id].ErrorThreshold)
                featureState = HealthState.Error;
            else if (roll < appThresholds[app.Id].DegradedThreshold)
                featureState = HealthState.Degraded;
            else
                featureState = HealthState.Operational;

            featureStates.Add(featureState);

            result.Inner.Add(new HealthCheckRecord
            {
                Feature = feature,
                State = featureState,
                Message = featureState switch
                {
                    HealthState.Operational => "OK",
                    HealthState.Degraded => "Slow or partial failure",
                    HealthState.Error => "Connection failed / Timeout",
                    _ => "Unknown"
                }
            });
        }

        // Derive overall state from features
        if (featureStates.Contains(HealthState.Error))
        {
            result.State = HealthState.Error;
            result.ResponseCode = 500;
            result.Message = "System failure detected";
            result.Fail = "One or more components failed";
        }
        else if (featureStates.Contains(HealthState.Degraded))
        {
            result.State = HealthState.Degraded;
            result.ResponseCode = 200;
            result.Message = "Minor issues detected";
            result.Fail = "Degraded components present";
        }
        else
        {
            result.State = HealthState.Operational;
            result.ResponseCode = 200;
            result.Message = "All systems functional";
        }

        return result;
    }

    private async Task InsertBatchAsync(List<MonitoringResult> batch)
    {
        _context.MonitoringResults.AddRange(batch);
        await _context.SaveChangesAsync();
    }
}
