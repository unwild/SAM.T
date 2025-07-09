using Microsoft.EntityFrameworkCore;
using SAM.T.Worker;
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
builder.Services.AddTransient<MonitoredApplicationsService>();
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

Routes.Register(app);

app.Run();