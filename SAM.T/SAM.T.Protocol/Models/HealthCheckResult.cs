using System.Collections.Generic;

namespace SAM.T.Protocol
{
    public class HealthCheckResult : HealthCheck
    {
        public HealthCheckResult(HealthState state, string message = null)
        {
            Feature = Constants.ApplicationRootFeature; // -> Top level feature = Application
            State = state;
            Message = message;
        }

        public List<HealthCheck> Inner { get; set; } = new List<HealthCheck>();
    }
}
