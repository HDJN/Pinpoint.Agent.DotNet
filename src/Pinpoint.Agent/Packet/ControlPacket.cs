namespace Pinpoint.Agent.Packet
{
    public class ControlPacket : BasicPacket
    {
        public int RequestId { get; set; }

        public ControlPacket(byte[] payload) : base(payload)
        {

        }

        public ControlPacket(int requestId, byte[] payload) : base(payload)
        {
            RequestId = requestId;
        }
    }
}
