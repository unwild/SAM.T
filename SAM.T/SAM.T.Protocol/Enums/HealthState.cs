namespace SAM.T.Protocol
{
    public enum HealthState
    {
        /// <summary>
        /// Application is running fine
        /// </summary>
        Operational,

        /// <summary>
        /// Application having some minor problems
        /// </summary>
        Degraded,

        /// <summary>
        /// Application is not able to do it's job
        /// </summary>
        Error
    }
}