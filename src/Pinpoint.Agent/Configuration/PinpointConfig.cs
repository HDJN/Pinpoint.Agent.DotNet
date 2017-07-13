namespace Pinpoint.Agent.DotNet.Configuration
{
    public class PinpointConfig
    {
        public string CollectorIp { get; set; }

        public int TcpListenPort { get; set; }

        public int UdpStatListenPort { get; set; }

        public int UpdSpanListenPort { get; set; }
    }
}
