using System;

namespace SAM.T.Protocol.Models
{
    public class MonitoringDetails : HealthCheck
    {
        public DateTime? LastStatusChange { get; set; }
        public HealthState LastStatusState { get; set; }
    }
}
