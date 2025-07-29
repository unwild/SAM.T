using Microsoft.EntityFrameworkCore;
using SAM.T.Worker.Data;

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

        public async static Task SeedDatabase(this WebApplication app)
        {
            var config = app.Configuration;

            if (!config.GetValue("Database:SeedTestData", false))
                return;

            using var serviceScope = app.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            var context = services.GetRequiredService<MonitoringContext>();

            var seeder = new MonitoringDataSeeder(context);
            await seeder.SeedAsync();       
        }
    }
}
