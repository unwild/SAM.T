namespace SAM.T.Protocol.Models
{
    public class ApplicationCreation
    {
        public string Name { get; set; } = string.Empty;

        public string Environment { get; set; } = string.Empty; 

        /// <summary>
        /// The URL of the application to monitor.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The application healthcheck endpoint.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        public bool UseProxy { get; set; } = false;

        public string? ProxyUrl { get; set; }

        public int? ProxyPort { get; set; }

        public string? ProxyUsername { get; set; }

        public string? ProxyPassword { get; set; }
    }
}
