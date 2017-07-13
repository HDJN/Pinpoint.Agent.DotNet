namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class ControlHandshakeResponsePacket : ControlPacket
    {
        public const String CODE = "code";
        public const String SUB_CODE = "subCode";

        public const String CLUSTER = "cluster";

        public short PacketType
        {
            get
            {
                return Common.PacketType.CONTROL_HANDSHAKE_RESPONSE;
            }
        }

        public ControlHandshakeResponsePacket(byte[] payload) : base(payload)
        {

        }

        public ControlHandshakeResponsePacket(int requestId, byte[] payload) : base(payload)
        {
            RequestId = requestId;
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort(Common.PacketType.CONTROL_HANDSHAKE_RESPONSE, buffer);
            BufferHelper.WriteInt(RequestId, buffer);

            return PayloadPacket.appendPayload(buffer, Payload);
        }

        public static ControlHandshakeResponsePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var messageId = BufferHelper.ReadInt(buffer);
            var payload = PayloadPacket.readPayload(buffer);
            var helloPacket = new ControlHandshakeResponsePacket(payload);
            helloPacket.RequestId = messageId;
            return helloPacket;
        }

        public override String ToString()
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
