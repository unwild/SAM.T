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

        app.MapGet("/application/{appId}", async (int appId, MonitoredApplicationsService applicationsService) =>
        {
            var application = await applicationsService.GetByIdAsync(appId);
            return application == null ? Results.NotFound() : Results.Json(application);
        })
        .WithName("Get application by id")
        .WithOpenApi();

        app.MapPut("/update/application/{appId}", async (int appId, ApplicationCreation appRequest, MonitoredApplicationsService applicationsService) =>
        {
            var updated = await applicationsService.UpdateAsync(appId, appRequest);
            return updated ? Results.Ok() : Results.NotFound();
        })
        .WithName("Update application by id")
        .WithOpenApi();

        app.MapDelete("/application/{appId}", async (int appId, MonitoredApplicationsService applicationsService) =>
        {
            var deleted = await applicationsService.DeleteAsync(appId);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("Delete application by id")
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
            return Results.Json(await monitoringService.GetMonitoringStates());
        })
        .WithName("Get applications top level monitoring data")
        .WithOpenApi();

        app.MapGet("/monitor/{appId}", async (int appId, MonitoringService monitoringService) =>
        {
            return Results.Json(await monitoringService.GetMonitoredApplicationInfo(appId));
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
