
using SAM.T.Protocol.Models;
using System;
using System.Collections.Generic;

namespace SAM.T.Protocol
{
    public class MonitoringState
    {
        public int ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationEnvironment { get; set; }

        public string ApplicationUrl { get; set; }

        public HealthState State { get; set; }

        public string Message { get; set; }

        public string Fail { get; set; }

        public long ResponseTime { get; set; }

        public ResponseTimeState ResponseTimeDeviation { get; set; }

        public DateTime LastUpdate { get; set; }
        
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
