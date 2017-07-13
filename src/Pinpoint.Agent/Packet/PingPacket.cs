namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class PingPacket : BasicPacket
    {
        public static PingPacket PING_PACKET = new PingPacket();

        // optional
        private int pingId;

        private byte stateVersion;
        private byte stateCode;

        private static byte[] PING_BYTE;

        static PingPacket()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort(Common.PacketType.CONTROL_PING, buffer);
            PING_BYTE = buffer.ToArray();
        }

        public PingPacket() :
            this(-1)
        {

        }

        public PingPacket(int pingId) :
            this(pingId, (byte)(-1 + 256), (byte)(-1 + 256))
        {

        }

        public PingPacket(int pingId, byte stateVersion, byte stateCode)
        {
            this.pingId = pingId;

            this.stateVersion = stateVersion;
            this.stateCode = stateCode;
        }

        public short getPacketType()
        {
            return PacketType.CONTROL_PING;
        }

        public byte[] toBuffer()
        {
            if (pingId == -1)
            {
                return PING_BYTE;
            }
            else
            {
                // 2 + 4 + 1 + 1
                var buffer = new MemoryStream();
                BufferHelper.WriteShort(PacketType.CONTROL_PING, buffer);
                BufferHelper.WriteInt(pingId, buffer);
                buffer.WriteByte(stateVersion);
                buffer.WriteByte(stateCode);
                return buffer.ToArray();
            }
        }

        public static PingPacket readBuffer(short packetType, NetworkStream buffer)
        {
            int pingId = BufferHelper.ReadInt(buffer);
            byte stateVersion = (byte)buffer.ReadByte();
            byte stateCode = (byte)buffer.ReadByte();

            return new PingPacket(pingId, stateVersion, stateCode);
        }

        public int getPingId()
        {
            return pingId;
        }

        public byte getStateVersion()
        {
            return stateVersion;
        }

        public byte getStateCode()
        {
            return stateCode;
        }

        public override String ToString()
        {
            if (pingId == -1)
            {
                return "PingPacket";
            }

            StringBuilder sb = new StringBuilder(32);
            sb.Append("PingPacket");

            if (pingId != -1)
            {
                sb.Append("{pingId:");
                sb.Append(pingId);
                sb.Append("(");
                sb.Append(stateCode);
                sb.Append(")");
                sb.Append("}");
            }

            return sb.ToString();
        }
    }
}
