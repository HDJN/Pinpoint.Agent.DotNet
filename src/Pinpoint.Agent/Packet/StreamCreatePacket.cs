namespace Pinpoint.Agent.Packet
{
    using Common;
    using System.IO;
    using System.Net.Sockets;

    public class StreamCreatePacket : BasicStreamPacket
    {
        private static short PACKET_TYPE = PacketType.APPLICATION_STREAM_CREATE;

        public byte[] Payload { get; set; }

        public StreamCreatePacket(int streamChannelId, byte[] payload)
            : base(streamChannelId)
        {
            this.Payload = payload;
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

            return PayloadPacket.appendPayload(buffer, Payload);
        }

        public static StreamCreatePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var streamChannelId = BufferHelper.ReadInt(buffer);
            var payload = PayloadPacket.readPayload(buffer);

            return new StreamCreatePacket(streamChannelId, payload);
        }
    }
}
