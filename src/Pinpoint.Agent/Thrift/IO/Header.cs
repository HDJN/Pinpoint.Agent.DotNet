namespace Pinpoint.Agent.Thrift.IO
{
    using System;

    public class Header
    {
        public byte Signature { get; set; } = 0xef;

        public sbyte Version { get; set; } = 0x10;

        public short Type { get; set; }

        public Header()
        {

        }

        public Header(byte signature, sbyte version, short type)
        {
            Signature = signature;
            Version = version;
            Type = type;
        }

        public override String ToString()
        {
            return "Header{" +
                    "signature=" + Signature +
                    ", version=" + Version +
                    ", type=" + Type +
                    '}';
        }
    }
}
