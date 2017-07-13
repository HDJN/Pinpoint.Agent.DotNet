namespace Pinpoint.Agent.Packet
{
    public class BasicStreamPacket
    {
        public int StreamChannelId { get; set; }

        public BasicStreamPacket(int streamChannelId)
        {
            StreamChannelId = streamChannelId;
        }
    }
}
