namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;

    public class ClientClosePacket : BasicPacket
    {
        public short PacketType
        {
            get
            {
                return (short)Common.PacketType.CONTROL_CLIENT_CLOSE;
            }
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort((short)Common.PacketType.CONTROL_CLIENT_CLOSE, buffer);
            return PayloadPacket.appendPayload(buffer, Payload);
        }

        public static ClientClosePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var payload = PayloadPacket.readPayload(buffer);
            if (payload == null)
            {
                return null;
            }
            var requestPacket = new ClientClosePacket();
            requestPacket.Payload = payload;
            return requestPacket;

        }

        public override String ToString()
        {
            return "ClientClosePacket";
        }
    }
}
