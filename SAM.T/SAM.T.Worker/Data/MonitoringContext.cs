﻿using Microsoft.EntityFrameworkCore;
using SAM.T.Worker.Data.Models;

namespace SAM.T.Worker.Data;

public class MonitoringContext(DbContextOptions<MonitoringContext> options) : DbContext(options)
{
    public DbSet<MonitoredApplication> MonitoredApplications { get; set; }

    public DbSet<MonitoringResult> MonitoringResults { get; set; }

    public DbSet<HealthCheckRecord> HealthCheckRecords { get; set; }

}
