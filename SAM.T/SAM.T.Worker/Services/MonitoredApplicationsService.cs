using SAM.T.Protocol.Models;
using SAM.T.Worker.Data;
using SAM.T.Worker.Data.Models;

namespace SAM.T.Worker.Services;

public class MonitoredApplicationsService
{
    private readonly MonitoringContext _context;

    public MonitoredApplicationsService(MonitoringContext context)
    {
        _context = context;
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
        return app.Id;
    }
}
