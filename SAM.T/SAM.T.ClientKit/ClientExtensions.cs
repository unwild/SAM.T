using SAM.T.Protocol;

namespace SAM.T.ClientKit
{
    public static class ClientExtensions
    {
        public static HealthCheckResult WithFeature(this HealthCheckResult hc, string featureIdentifier, HealthState state)
        {
            return hc.WithFeature(featureIdentifier, state, null);
        }

        public static HealthCheckResult WithFeature(this HealthCheckResult hc, string featureIdentifier, HealthState state, string message)
        {
            hc.Inner.Add(new HealthCheck { Feature = featureIdentifier, Message = message, State = state });

            return hc;
        }
    }
}
