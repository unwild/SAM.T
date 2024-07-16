using System.ComponentModel.DataAnnotations;

namespace SAM.T.Worker.Data.Models;

public class MonitoredApplication
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Url { get; set; }

    public required bool UseProxy { get; set; } = false;

    public string? ProxyUrl { get; set; }

    public int? ProxyPort { get; set; }

    public string? ProxyUsername { get; set; }

    public string? ProxyPassword { get; set; }
}
