namespace SAM.T.Protocol.Models
{
    public class ApplicationCreation
    {
        public string Name { get; set; }

        public string Environment { get; set; }

        /// <summary>
        /// The URL of the application to monitor.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The application healthcheck endpoint.
        /// </summary>
        public string Endpoint { get; set; }

        public bool UseProxy { get; set; } = false;

        public string ProxyUrl { get; set; }

        public int? ProxyPort { get; set; }

        public string ProxyUsername { get; set; }

        public string ProxyPassword { get; set; }
    }
}
