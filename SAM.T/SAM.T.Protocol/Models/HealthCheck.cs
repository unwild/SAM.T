
namespace SAM.T.Protocol
{
    public class HealthCheck
    {
        public string Feature { get; set; }

        public HealthState State { get; set; }

        public string? Message { get; set; }
    }
}
