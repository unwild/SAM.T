using SAM.T.Protocol;
using System.ComponentModel.DataAnnotations;

namespace SAM.T.Worker.Data.Models;

public class HealthCheckRecord
{
    [Key]
    public int Id { get; set; }

    public required string Feature { get; set; }

    public HealthState State { get; set; }

    public string? Message { get; set; }

    public int MonitoringResultId { get; set; }

    public MonitoringResult MonitoringResult { get; set; } = null!;

}
