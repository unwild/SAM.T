using SAM.T.ClientKit;
using SAM.T.Protocol;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();


app.MapGet("/health", () =>
{
    if (DateTime.Now.Ticks % 3 == 0)
        return Results.Problem();

    return Results.Ok(new HealthCheckResult(HealthState.Operational)
        .WithFeature("Database", HealthState.Degraded)
        .WithFeature("Daily import", HealthState.Operational, $"Import successfully performed @{DateTime.Now.Add(-DateTime.Now.TimeOfDay)}"));
});

app.Run();
