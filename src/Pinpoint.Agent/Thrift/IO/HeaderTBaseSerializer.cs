namespace Pinpoint.Agent.Thrift.IO
{
    using global::Thrift.Protocol;
    using global::Thrift.Transport;
    using System;
    using System.IO;

    public class HeaderTBaseSerializer : IDisposable
    {
        private static readonly String UTF8 = "UTF8";

        private readonly MemoryStream baos;
        private readonly TProtocol protocol;
        private readonly DefaultTBaseLocator locator;

        public HeaderTBaseSerializer()
        {
            baos = new MemoryStream();
            protocol = new TCompactProtocol.Factory().GetProtocol(new TStreamTransport(baos, baos));
            locator = new DefaultTBaseLocator();
        }

        public byte[] serialize(TBase @base)
        {
            var header = locator.HeaderLookup(@base);
            Reset();
            WriteHeader(header);
            @base.Write(protocol);
            return baos.ToArray();
        }

        public void Reset()
        {
            baos.Seek(0, SeekOrigin.Begin);
            baos.SetLength(0);
        }

        private void WriteHeader(Header header)
        {
            protocol.WriteByte((sbyte)header.Signature);
            protocol.WriteByte(header.Version);
            short type = header.Type;
            protocol.WriteByte((sbyte)(type >> 8));
            protocol.WriteByte((sbyte)(type));
        }

        public void Dispose()
        {
            if(baos != null)
            {
                baos.Dispose();
            }
        }
    }
}
