namespace Pinpoint.Agent.Packet
{
    using Common;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class ControlHandshakePacket : ControlPacket
    {
        public short getPacketType
        {
            get
            {
                return (short)Common.PacketType.CONTROL_HANDSHAKE;
            }
        }

        public ControlHandshakePacket(byte[] payload) : base(payload)
        {

        }

        public ControlHandshakePacket(int requestId, byte[] payload) : base(requestId, payload)
        {

        }



        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort((short)Common.PacketType.CONTROL_HANDSHAKE, buffer);
            BufferHelper.WriteInt(RequestId, buffer);

            return PayloadPacket.appendPayload(buffer, Payload);
        }

        public static ControlHandshakePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var messageId = BufferHelper.ReadInt(buffer);
            var payload = PayloadPacket.readPayload(buffer);
            var helloPacket = new ControlHandshakePacket(payload);
            helloPacket.RequestId = messageId;
            return helloPacket;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().Name);
            sb.Append("{requestId=").Append(RequestId);
            sb.Append(", ");
            if (Payload == null)
            {
                sb.Append("payload=null");
            }
            else
            {
                sb.Append("payloadLength=").Append(Payload.Length);
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
