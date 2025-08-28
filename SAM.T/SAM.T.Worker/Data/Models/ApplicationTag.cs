using System.ComponentModel.DataAnnotations;

namespace SAM.T.Worker.Data.Models;

public class ApplicationTag
{
    [Key]
    public int Id { get; set; }
    
    public required string Key { get; set; }
    
    public required string Value { get; set; }
    
    public int MonitoredApplicationId { get; set; }
    
    public MonitoredApplication MonitoredApplication { get; set; } = null!;
}