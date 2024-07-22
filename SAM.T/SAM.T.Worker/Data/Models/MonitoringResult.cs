using SAM.T.Protocol;
using System.ComponentModel.DataAnnotations;

namespace SAM.T.Worker.Data.Models;

public class MonitoringResult
{
    [Key]
    public int Id { get; set; }

    public DateTime Time { get; set; }

    public int ResponseCode { get; set; }

    public HealthState State { get; set; }

    public string? Message { get; set; }

    public string? Fail { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTime { get; set; }

    public double ResponseTimeDeviation { get; set; }

    public int MonitoredApplicationId { get; set; }

    public MonitoredApplication MonitoredApplication { get; set; } = null!;

    public ICollection<HealthCheckRecord> Inner { get; set; } = [];

}
