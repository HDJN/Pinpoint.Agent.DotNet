namespace Pinpoint.Agent.DotNet.Configuration
{
    public class AgentConfig
    {
        public string HostName { get; set; }

        public string AgentId { get; set; }

        public string ApplicationName { get; set; }

        public long AgentStartTime { get; set; }

        public string AgentVersion { get; set; }
    }
}
