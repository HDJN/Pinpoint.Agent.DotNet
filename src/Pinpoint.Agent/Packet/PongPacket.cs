namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;

    public class PongPacket : BasicPacket
    {
        public static readonly PongPacket PONG_PACKET = new PongPacket();

        private static byte[] PONG_BYTE;

        public short PacketType
        {
            get
            {
                return Common.PacketType.CONTROL_PONG;
            }
        }

        static PongPacket()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort(Common.PacketType.CONTROL_PONG, buffer);
            PONG_BYTE = buffer.ToArray();
        }

        public PongPacket()
        {

        }

        public byte[] ToBuffer()
        {
            return PONG_BYTE;
        }

        public static PongPacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            return PONG_PACKET;
        }

        public override String ToString()
        {
            return "PongPacket";
        }
    }
}
