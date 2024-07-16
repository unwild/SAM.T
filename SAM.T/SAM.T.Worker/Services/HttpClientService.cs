using SAM.T.Worker.Data.Models;
using System.Net;

namespace SAM.T.Worker.Services;

public class HttpClientService
{

    private readonly Dictionary<int, HttpClient> cachedClients = [];

    public HttpClient Get(MonitoredApplication app)
    {
        return GetOrCreate(app);
    }

    private HttpClient GetOrCreate(MonitoredApplication app)
    {
        var key = GetKey(app);

        if (cachedClients.TryGetValue(key, out var storedClient))
            return storedClient;

        var client = Create(app);

        cachedClients.Add(key, client);

        return client;
    }

    private static HttpClient Create(MonitoredApplication app)
    {
        HttpClientHandler handler = new();

        if (app.UseProxy)
        {
            var proxy = new WebProxy { Address = new Uri($"{app.ProxyUrl}:{app.ProxyPort}") };

            if (app.ProxyUsername is not null)
                proxy.Credentials = new NetworkCredential(app.ProxyUsername, app.ProxyPassword);

            handler.UseProxy = true;
            handler.Proxy = proxy;
        }

        return new HttpClient(handler);
    }

    private static int GetKey(MonitoredApplication app)
    {
        if (!app.UseProxy)
            return 0;

        return $"{app.ProxyUrl}|{app.ProxyPort}|{app.ProxyUsername}|{app.ProxyPassword}".GetHashCode();
    }
}
