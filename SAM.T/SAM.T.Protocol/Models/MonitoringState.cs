
namespace SAM.T.Protocol
{
    public class MonitoringState
    {
        public string ApplicationName { get; set; }

        public HealthState State { get; set; }

        public string Message { get; set; }

        public string Fail { get; set; }

        public ResponseTimeState ResponseTimeDeviation { get; set; }
    }
}
