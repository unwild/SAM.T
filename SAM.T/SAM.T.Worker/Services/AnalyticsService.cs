using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using SAM.T.Worker.Data;

namespace SAM.T.Worker.Services;

public class AnalyticsService
{
    private readonly MonitoringContext _context;

    public AnalyticsService(MonitoringContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Analyse latest monitoring results and store the results
    /// </summary>
    /// <returns></returns>
    public async Task Analyse()
    {
        var monitoredApplicationsIds = await _context.MonitoredApplications.Select(ma => ma.Id).ToListAsync();

        foreach (var appId in monitoredApplicationsIds)
        {
            // Get standard deviation on the previous results
            var responseTimes = await _context.MonitoringResults
                .Where(mr => mr.MonitoredApplicationId == appId)
                .OrderByDescending(mr => mr.Time)
                .Skip(1)
                .Select(mr => (double)mr.ResponseTime)
                .ToListAsync();

            var lastResponseTime = await _context.MonitoringResults
                .Where(mr => mr.MonitoredApplicationId == appId)
                .OrderByDescending(mr => mr.Time)
                .FirstAsync();

            // We need at least 5 results to have data to analyse
            if (responseTimes.Count < 5)
                continue;

            // Calculate and store current deviation factor
            var avg = responseTimes.Average();
            var stdDev = responseTimes.PopulationStandardDeviation();

            var dev = (lastResponseTime.ResponseTime - avg) / stdDev;

            lastResponseTime.ResponseTimeDeviation = dev;

            await _context.SaveChangesAsync();
        }
    }

}
