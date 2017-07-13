namespace Pinpoint.Agent.Packet
{
    using Common;
    using System.IO;
    using System.Net.Sockets;

    public class StreamClosePacket : BasicStreamPacket
    {
        private static short PACKET_TYPE = PacketType.APPLICATION_STREAM_CLOSE;

        public StreamCode Code { get; set; }

        public StreamClosePacket(int streamChannelId, StreamCode code)
            : base(streamChannelId)
        {
            this.Code = code;
        }

        public short getPacketType()
        {
            return PACKET_TYPE;
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort(PACKET_TYPE, buffer);
            BufferHelper.WriteInt(StreamChannelId, buffer);
            BufferHelper.WriteShort((short)Code, buffer);

            return buffer.ToArray();
        }

        public static StreamClosePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var streamChannelId = BufferHelper.ReadInt(buffer);
            var code = (StreamCode)BufferHelper.ReadShort(buffer);

            return new StreamClosePacket(streamChannelId, code);
        }
    }
}
