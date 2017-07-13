namespace Pinpoint.Agent.Packet
{
    using Common;
    using System.IO;
    using System.Net.Sockets;

    public class StreamCreateSuccessPacket : BasicStreamPacket
    {
        private static short PACKET_TYPE = PacketType.APPLICATION_STREAM_CREATE_SUCCESS;

        public StreamCreateSuccessPacket(int streamChannelId)
            : base(streamChannelId)
        {

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

            return buffer.ToArray();
        }

        public static StreamCreateSuccessPacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var streamChannelId = BufferHelper.ReadInt(buffer);

            return new StreamCreateSuccessPacket(streamChannelId);
        }
    }
}
