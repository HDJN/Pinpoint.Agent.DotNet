namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class ResponsePacket : BasicPacket
    {
        public int RequestId { get; set; }

        public short PacketType
        {
            get
            {
                return Common.PacketType.APPLICATION_RESPONSE;
            }
        }

        public ResponsePacket()
        {

        }

        public ResponsePacket(byte[] payload) : base(payload)
        {

        }

        public ResponsePacket(int requestId, byte[] payload) : base(payload)
        {
            RequestId = requestId;
        }

        public byte[] ToBuffer()
        {
            var buffer = new MemoryStream();
            BufferHelper.WriteShort((short)Common.PacketType.APPLICATION_RESPONSE, buffer);
            BufferHelper.WriteInt(RequestId, buffer);

            return PayloadPacket.appendPayload(buffer, Payload);
        }


        public static ResponsePacket ReadBuffer(short packetType, NetworkStream buffer)
        {
            var messageId = BufferHelper.ReadInt(buffer);
            var payload = PayloadPacket.readPayload(buffer);
            var responsePacket = new ResponsePacket(payload);
            responsePacket.RequestId = messageId;
            return responsePacket;

        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ResponsePacket");
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
