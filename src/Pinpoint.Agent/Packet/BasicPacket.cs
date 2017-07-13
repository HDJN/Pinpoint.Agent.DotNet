namespace Pinpoint.Agent.Packet
{
    using System;

    public class BasicPacket
    {
        public byte[] Payload { get; set; }

        protected BasicPacket()
        {

        }

        public BasicPacket(byte[] payload)
        {
            if (payload == null)
            {
                throw new NullReferenceException("payload");
            }
            this.Payload = payload;
        }
    }
}
