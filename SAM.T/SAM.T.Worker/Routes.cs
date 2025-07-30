using Microsoft.AspNetCore.Mvc;
using SAM.T.Protocol.Models;
using SAM.T.Worker.Services;

namespace SAM.T.Worker;

public static class Routes
{
    public static void Register(WebApplication app)
    {
        app.MapPost("/application", async (ApplicationCreation app, MonitoredApplicationsService applicationsService) =>
        {
            var createdApp = await applicationsService.Create(app);
            return Results.Json(createdApp, statusCode: 201);
        })
        .WithName("Create a new application to monitor")
        .WithOpenApi();

        app.MapPost("/trigger", async (MonitoringService monitoringService) =>
        {
            await monitoringService.Execute();
            return Results.Ok();
        })
        .WithName("Trigger a new monitoring tour")
        .WithOpenApi();

        app.MapGet("/monitor", async (MonitoringService monitoringService) =>
        {
            return Results.Json(await monitoringService.GetRecords());
        })
        .WithName("Get applications top level monitoring data")
        .WithOpenApi();

        app.MapGet("/monitor/{appId}", async (int appId, MonitoringService monitoringService) =>
        {
            return Results.Json(await monitoringService.GetDetails(appId));
        })
        .WithName("Get application in-depth monitoring data")
        .WithOpenApi();

        app.MapGet("/availability/{appId}", async (int appId, MonitoringService monitoringService, [FromQuery] int days = 30) =>
        {
            return Results.Json(await monitoringService.GetAvailability(appId, days));
        })
        .WithName("Get application availability records")
        .WithOpenApi();

    }
}
