using Microsoft.EntityFrameworkCore;
using SAM.T.Worker.Configuration;
using SAM.T.Worker.Data;
using SAM.T.Worker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "dashboardPolicy", policy =>
    {
        policy.WithOrigins(config.GetValue<string>("FrontUrl" ?? throw new ArgumentException("Front url is not defined in application configuration !"))!);
    });
});

builder.Services.AddSingleton<HttpClientService>();
builder.Services.AddTransient<MonitoringService>();
builder.Services.AddTransient<AnalyticsService>();

builder.Services.AddDbContext<MonitoringContext>(opt
    => opt.UseNpgsql(config.GetConnectionString("Database") ?? throw new ArgumentException("Connection string is not defined in application configuration !")));

var app = builder.Build();

app.MigrateDatabase();
app.SeedDatabase();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("dashboardPolicy");

app.UseHttpsRedirection();

HandleRoutes(app);

app.Run();


static void HandleRoutes(WebApplication app)
{
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
}