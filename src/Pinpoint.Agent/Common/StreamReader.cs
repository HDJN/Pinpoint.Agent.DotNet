namespace Pinpoint.Agent.Common
{
    using System;
    using System.IO;

    public class BufferHelper
    {
        private static ByteOrder byteOrder = ByteOrder.BIG_ENDIAN;

        public static void WriteShort(short value, Stream buffer)
        {
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                buffer.WriteByte((byte)(value >> 8));
                buffer.WriteByte((byte)value);
            }
            else
            {
                buffer.WriteByte((byte)value);
                buffer.WriteByte((byte)(value >> 8));
            }
        }

        public static void WriteInt(int value, Stream buffer)
        {
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                buffer.WriteByte((byte)(value >> 24));
                buffer.WriteByte((byte)(value >> 16));
                buffer.WriteByte((byte)(value >> 8));
                buffer.WriteByte((byte)value);
            }
            else
            {
                buffer.WriteByte((byte)value);
                buffer.WriteByte((byte)(value >> 8));
                buffer.WriteByte((byte)(value >> 16));
                buffer.WriteByte((byte)(value >> 24));
            }
        }

        public static void WriteLong(long value, Stream buffer)
        {
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                buffer.WriteByte((byte)(value >> 56));
                buffer.WriteByte((byte)(value >> 48));
                buffer.WriteByte((byte)(value >> 40));
                buffer.WriteByte((byte)(value >> 32));
                buffer.WriteByte((byte)(value >> 24));
                buffer.WriteByte((byte)(value >> 16));
                buffer.WriteByte((byte)(value >> 8));
                buffer.WriteByte((byte)value);
            }
            else
            {
                buffer.WriteByte((byte)value);
                buffer.WriteByte((byte)(value >> 8));
                buffer.WriteByte((byte)(value >> 16));
                buffer.WriteByte((byte)(value >> 24));
                buffer.WriteByte((byte)(value >> 32));
                buffer.WriteByte((byte)(value >> 40));
                buffer.WriteByte((byte)(value >> 48));
                buffer.WriteByte((byte)(value >> 56));
            }
        }

        public static short ReadShort(Stream buffer)
        {
            var bytes = new byte[2];
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                bytes[1] = (byte)buffer.ReadByte();
                bytes[0] = (byte)buffer.ReadByte();
                return BitConverter.ToInt16(bytes, 0);
            }
            else
            {
                bytes[0] = (byte)buffer.ReadByte();
                bytes[1] = (byte)buffer.ReadByte();
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        public static int ReadInt(Stream buffer)
        {
            var bytes = new byte[4];
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                bytes[3] = (byte)buffer.ReadByte();
                bytes[2] = (byte)buffer.ReadByte();
                bytes[1] = (byte)buffer.ReadByte();
                bytes[0] = (byte)buffer.ReadByte();
                return BitConverter.ToInt32(bytes, 0);
            }
            else
            {
                bytes[0] = (byte)buffer.ReadByte();
                bytes[1] = (byte)buffer.ReadByte();
                bytes[2] = (byte)buffer.ReadByte();
                bytes[3] = (byte)buffer.ReadByte();
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public static long ReadLong(Stream buffer)
        {
            var bytes = new byte[8];
            if (byteOrder == ByteOrder.BIG_ENDIAN)
            {
                bytes[7] = (byte)buffer.ReadByte();
                bytes[6] = (byte)buffer.ReadByte();
                bytes[5] = (byte)buffer.ReadByte();
                bytes[4] = (byte)buffer.ReadByte();
                bytes[3] = (byte)buffer.ReadByte();
                bytes[2] = (byte)buffer.ReadByte();
                bytes[1] = (byte)buffer.ReadByte();
                bytes[0] = (byte)buffer.ReadByte();
                return BitConverter.ToInt64(bytes, 0);
            }
            else
            {
                bytes[0] = (byte)buffer.ReadByte();
                bytes[1] = (byte)buffer.ReadByte();
                bytes[2] = (byte)buffer.ReadByte();
                bytes[3] = (byte)buffer.ReadByte();
                bytes[4] = (byte)buffer.ReadByte();
                bytes[5] = (byte)buffer.ReadByte();
                bytes[6] = (byte)buffer.ReadByte();
                bytes[7] = (byte)buffer.ReadByte();
                return BitConverter.ToInt64(bytes, 0);
            }
        }
    }
}
