using SAM.T.Protocol.Models;
using SAM.T.Worker.Data;
using SAM.T.Worker.Data.Models;

namespace SAM.T.Worker.Services;

public class MonitoredApplicationsService
{
    private readonly MonitoringContext _context;
    private readonly MonitoringService _monitoringService;

    public MonitoredApplicationsService(MonitoringContext context, MonitoringService monitoringService)
    {
        _context = context;
        _monitoringService = monitoringService;
    }

    public async Task<int> Create(ApplicationCreation appRequest)
    {
        var app = new MonitoredApplication
        {
            Name = appRequest.Name,
            Environment = appRequest.Environment,
            Url = appRequest.Url,
            Endpoint = appRequest.Endpoint,
            UseProxy = appRequest.UseProxy,
            ProxyUrl = appRequest.ProxyUrl,
            ProxyPort = appRequest.ProxyPort,
            ProxyUsername = appRequest.ProxyUsername,
            ProxyPassword = appRequest.ProxyPassword
        };

        _context.MonitoredApplications.Add(app);

        await _context.SaveChangesAsync();

        // Trigger immediate monitoring for the newly created application
        await _monitoringService.Execute(app.Id);

        return app.Id;
    }
}
