namespace Pinpoint.Agent.Thrift.IO
{
    using global::Thrift.Protocol;
    using global::Thrift.Transport;
    using System;
    using System.IO;

    public class HeaderTBaseDeserializer
    {
        private static readonly String UTF8 = "UTF8";

        private MemoryStream baos;
        private readonly TProtocol protocol;
        private readonly DefaultTBaseLocator locator;

        public HeaderTBaseDeserializer()
        {
            baos = new MemoryStream();
            protocol = new TCompactProtocol.Factory().GetProtocol(new TStreamTransport(baos, baos));
            locator = new DefaultTBaseLocator();
        }

        public TBase Deserialize(byte[] buffer)
        {
            Reset();
            baos.Write(buffer, 0, buffer.Length);
            baos.Seek(0, SeekOrigin.Begin);
            baos = new MemoryStream(buffer);
            Header header = ReadHeader();
            TBase @base = locator.TBaseLookup(header.Type);
            @base.Read(protocol);
            return @base;
        }

        private Header ReadHeader()
        {
            var signature = (byte)protocol.ReadByte();
            var version = protocol.ReadByte();

            // fixed size regardless protocol
            var type1 = protocol.ReadByte();
            var type2 = protocol.ReadByte();
            short type = bytesToShort(type1, type2);
            return new Header(signature, version, type);
        }

        private short bytesToShort(sbyte byte1, sbyte byte2)
        {
            return (short)(((byte1 & 0xff) << 8) | ((byte2 & 0xff)));
        }

        private void Reset()
        {
            baos.Seek(0, SeekOrigin.Begin);
            baos.SetLength(0);
        }
    }
}
