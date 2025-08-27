using Microsoft.EntityFrameworkCore;
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

    public async Task<ApplicationCreation?> GetByIdAsync(int appId)
    {
        var app = await _context.MonitoredApplications.FindAsync(appId);
        
        if (app == null)
            return null;

        return new ApplicationCreation
        {
            Name = app.Name,
            Environment = app.Environment,
            Url = app.Url,
            Endpoint = app.Endpoint,
            UseProxy = app.UseProxy,
            ProxyUrl = app.ProxyUrl,
            ProxyPort = app.ProxyPort,
            ProxyUsername = app.ProxyUsername,
            ProxyPassword = app.ProxyPassword
        };
    }

    public async Task<bool> UpdateAsync(int appId, ApplicationCreation appRequest)
    {
        var app = await _context.MonitoredApplications.FindAsync(appId);
        
        if (app == null)
            return false;

        app.Name = appRequest.Name;
        app.Environment = appRequest.Environment;
        app.Url = appRequest.Url;
        app.Endpoint = appRequest.Endpoint;
        app.UseProxy = appRequest.UseProxy;
        app.ProxyUrl = appRequest.ProxyUrl;
        app.ProxyPort = appRequest.ProxyPort;
        app.ProxyUsername = appRequest.ProxyUsername;
        app.ProxyPassword = appRequest.ProxyPassword;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int appId)
    {
        var app = await _context.MonitoredApplications.FindAsync(appId);
        
        if (app == null)
            return false;

        _context.MonitoredApplications.Remove(app);
        await _context.SaveChangesAsync();

        return true;
    }
}
