namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    public class RequestPacket : BasicPacket
    {
        public int RequestId { get; set; }

        public short PacketType
        {
            get
            {
                return (short)Common.PacketType.APPLICATION_REQUEST;
            }
        }

        public RequestPacket()
        {

        }

        public RequestPacket(byte[] payload) : base(payload)
        {

        }

        public RequestPacket(int requestId, byte[] payload) : base(payload)
        {
            RequestId = requestId;
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort((short)Common.PacketType.APPLICATION_REQUEST, buffer);
            BufferHelper.WriteInt(RequestId, buffer);

            return PayloadPacket.appendPayload(buffer, Payload);
        }


        public static RequestPacket readBuffer(short packetType, NetworkStream buffer)
        {
            var messageId = BufferHelper.ReadInt(buffer);
            var payload = PayloadPacket.readPayload(buffer);
            var reqeustPacket = new RequestPacket(payload);
            reqeustPacket.RequestId = messageId;
            return reqeustPacket;
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("RequestPacket");
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
