namespace Pinpoint.Agent.Packet
{
    using Common;
    using System;
    using System.IO;

    public class PayloadPacket
    {
        public static byte[] readPayload(Stream buffer)
        {
            var payloadLength = BufferHelper.ReadInt(buffer);
            if (payloadLength <= 0)
            {
                return new byte[0];
            }

            var payload = new byte[payloadLength];
            buffer.Read(payload, 0, payloadLength);

            return payload;
        }


        public static byte[] appendPayload(MemoryStream buffer, byte[] payload)
        {
            if (payload == null)
            {
                // this is also payload header
                BufferHelper.WriteInt(-1, buffer);
                return buffer.ToArray();
            }
            else
            {
                BufferHelper.WriteInt(payload.Length, buffer);
                buffer.Write(payload, 0, payload.Length);
                return buffer.ToArray();
            }
        }
    }
}
