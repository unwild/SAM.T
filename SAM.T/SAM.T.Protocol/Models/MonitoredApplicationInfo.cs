using System;

namespace SAM.T.Protocol.Models
{
    public class MonitoredApplicationInfo {
        public MonitoringState ApplicationState { get; set; }
        public MonitoringDetails[] Details { get; set; }
    }

    public class MonitoringDetails : HealthCheck
    {
        public DateTime? LastStatusChange { get; set; }
        public HealthState LastStatusState { get; set; }
    }
}
