using Microsoft.EntityFrameworkCore;
using SAM.T.Worker.Data;
using SAM.T.Worker.Data.Models;

namespace SAM.T.Worker.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void MigrateDatabase(this WebApplication app)
        {
            var config = app.Configuration;

            //Auto migrate
            if (config.GetValue("Database:AutoMigrate", false))
            {
                using var serviceScope = app.Services.CreateScope();
                var services = serviceScope.ServiceProvider;

                var context = services.GetRequiredService<MonitoringContext>();
                context.Database.Migrate();
            }
        }

        public static void SeedDatabase(this WebApplication app)
        {
            var config = app.Configuration;

            if (!config.GetValue("Database:SeedTestData", false))
                return;

            using var serviceScope = app.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            var context = services.GetRequiredService<MonitoringContext>();

            // Do not insert if data is already in database
            if (context.MonitoredApplications.Any())
                return;

            context.MonitoredApplications.Add(new MonitoredApplication { 
                Name = "ExampleApp",
                Environment = "dev",
                Url = "https://localhost:7042",
                Endpoint = "/health",
                UseProxy = false 
            });

            context.MonitoredApplications.Add(new MonitoredApplication {
                Name = "Google",
                Environment = "prod",
                Url = "https://google.com",
                Endpoint = "https://google.com",
                UseProxy = false 
            });

            context.SaveChanges();
        }
    }
}
