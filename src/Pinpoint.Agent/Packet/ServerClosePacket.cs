namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;

    public class ServerClosePacket : BasicPacket
    {
        public short PacketType()
        {
            return (short)Common.PacketType.CONTROL_SERVER_CLOSE;
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort((short)Common.PacketType.CONTROL_SERVER_CLOSE, buffer);

            return PayloadPacket.appendPayload(buffer, Payload);
        }

        public static ServerClosePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var payload = PayloadPacket.readPayload(buffer);
            if (payload == null)
            {
                return null;
            }
            var requestPacket = new ServerClosePacket();
            requestPacket.Payload = payload;
            return requestPacket;

        }

        public override String ToString()
        {
            return "ServerClosePacket";
        }
    }
}
