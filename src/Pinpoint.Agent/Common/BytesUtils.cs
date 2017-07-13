namespace Pinpoint.Agent.Common
{
    using System;
    using System.Text;

    public class BytesUtils
    {
        public static readonly int SHORT_BYTE_LENGTH = 2;
        public static readonly int INT_BYTE_LENGTH = 4;
        public static readonly int LONG_BYTE_LENGTH = 8;
        public static readonly int LONG_LONG_BYTE_LENGTH = 16;

        public static readonly int VLONG_MAX_SIZE = 10;
        public static readonly int VINT_MAX_SIZE = 5;

        private static readonly byte[] EMPTY_BYTES = new byte[0];
        private static readonly Encoding UTF8_CHARSET = Encoding.UTF8;

        private BytesUtils()
        {
        }

        public static byte[] stringLongLongToBytes(String str, int maxStringSize, long value1, long value2)
        {
            if (str == null)
            {
                throw new NullReferenceException("string must not be null");
            }
            if (maxStringSize < 0)
            {
                throw new IndexOutOfRangeException("maxStringSize");
            }
            var stringBytes = toBytes(str);
            if (stringBytes.Length > maxStringSize)
            {
                throw new IndexOutOfRangeException("string is max " + stringBytes.Length + ", string='" + str + "'");
            }
            var buffer = new byte[LONG_LONG_BYTE_LENGTH + maxStringSize];
            writeBytes(buffer, 0, stringBytes);
            writeFirstLong0(value1, buffer, maxStringSize);
            writeSecondLong0(value2, buffer, maxStringSize);
            return buffer;
        }

        public static int writeBytes(byte[] buffer, int bufferOffset, byte[] srcBytes)
        {
            if (srcBytes == null)
            {
                throw new NullReferenceException("srcBytes must not be null");
            }
            return writeBytes(buffer, bufferOffset, srcBytes, 0, srcBytes.Length);
        }

        public static int writeBytes(byte[] buffer, int bufferOffset, byte[] srcBytes, int srcOffset, int srcLength)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("buffer must not be null");
            }
            if (srcBytes == null)
            {
                throw new NullReferenceException("stringBytes must not be null");
            }
            if (bufferOffset < 0)
            {
                throw new IndexOutOfRangeException("negative bufferOffset:" + bufferOffset);
            }
            if (srcOffset < 0)
            {
                throw new IndexOutOfRangeException("negative srcOffset offset:" + srcOffset);
            }
            Array.Copy(srcBytes, srcOffset, buffer, bufferOffset, srcLength);
            return bufferOffset + srcLength;
        }

        public static long bytesToLong(byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + LONG_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 8));
            }

            var rv = (((long)buf[offset] & 0xff) << 56)
                    | (((long)buf[offset + 1] & 0xff) << 48)
                    | (((long)buf[offset + 2] & 0xff) << 40)
                    | (((long)buf[offset + 3] & 0xff) << 32)
                    | (((long)buf[offset + 4] & 0xff) << 24)
                    | (((long)buf[offset + 5] & 0xff) << 16)
                    | (((long)buf[offset + 6] & 0xff) << 8)
                    | (((long)buf[offset + 7] & 0xff));
            return rv;
        }

        public static int bytesToInt(byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + INT_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 4));
            }

            var v = ((buf[offset] & 0xff) << 24)
                    | ((buf[offset + 1] & 0xff) << 16)
                    | ((buf[offset + 2] & 0xff) << 8)
                    | ((buf[offset + 3] & 0xff));

            return v;
        }

        public static short bytesToShort(byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + SHORT_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 2));
            }

            var v = (short)(((buf[offset] & 0xff) << 8) | ((buf[offset + 1] & 0xff)));

            return v;
        }


        public static int bytesToSVar32(byte[] buffer, int offset)
        {
            return zigzagToInt(bytesToVar32(buffer, offset));
        }

        public static int bytesToVar32(byte[] buffer, int offset)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("buffer must not be null");
            }
            checkBound(buffer.Length, offset);

            // borrowing the protocol buffer's concept of variable-length encoding
            // copy https://github.com/google/protobuf 2.6.1
            // CodedInputStream.java -> int readRawVarint32()

            // See implementation notes for readRawVarint64
            fastpath:
            {
                int pos = offset;
                var bufferSize = buffer.Length;
                if (bufferSize == pos)
                {
                    goto fastpath;
                }

                int x;
                if ((x = buffer[pos++]) >= 0)
                {
                    return x;
                }
                else if (bufferSize - pos < 9)
                {
                    goto fastpath;
                }
                else if ((x ^= (buffer[pos++] << 7)) < 0)
                {
                    x ^= (~0 << 7);
                }
                else if ((x ^= (buffer[pos++] << 14)) >= 0)
                {
                    x ^= (~0 << 7) ^ (~0 << 14);
                }
                else if ((x ^= (buffer[pos++] << 21)) < 0)
                {
                    x ^= (~0 << 7) ^ (~0 << 14) ^ (~0 << 21);
                }
                else
                {
                    int y = buffer[pos++];
                    x ^= y << 28;
                    x ^= (~0 << 7) ^ (~0 << 14) ^ (~0 << 21) ^ (~0 << 28);
                    if (y < 0 &&
                            buffer[pos++] < 0 &&
                            buffer[pos++] < 0 &&
                            buffer[pos++] < 0 &&
                            buffer[pos++] < 0 &&
                            buffer[pos] < 0)
                    {
                        goto fastpath;  // Will throw malformedVarint()
                    }
                }

                return x;
            }
            return (int)readVar64SlowPath(buffer, offset);
        }

        public static long bytesToSVar64(byte[] buffer, int offset)
        {
            return zigzagToLong(bytesToVar64(buffer, offset));
        }

        public static long bytesToVar64(byte[] buffer, int offset)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("buffer must not be null");
            }
            checkBound(buffer.Length, offset);
            // borrowing the protocol buffer's concept of variable-length encoding
            // copy https://github.com/google/protobuf 2.6.1
            // CodedInputStream.java -> int readRawVarint32()

            // Implementation notes:
            //
            // Optimized for one-byte values, expected to be common.
            // The particular code below was selected from various candidates
            // empirically, by winning VarintBenchmark.
            //
            // Sign extension of (signed) Java bytes is usually a nuisance, but
            // we exploit it here to more easily obtain the sign of bytes read.
            // Instead of cleaning up the sign extension bits by masking eagerly,
            // we delay until we find the final (positive) byte, when we clear all
            // accumulated bits with one xor.  We depend on javac to constant fold.
            fastpath:
            {
                int pos = offset;
                int bufferSize = buffer.Length;
                if (bufferSize == pos)
                {
                    goto fastpath;
                }

                long x;
                int y;
                if ((y = buffer[pos++]) >= 0)
                {
                    return y;
                }
                else if (bufferSize - pos < 9)
                {
                    goto fastpath;
                }
                else if ((x = y ^ (buffer[pos++] << 7)) < 0L)
                {
                    x ^= (~0L << 7);
                }
                else if ((x ^= (buffer[pos++] << 14)) >= 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14);
                }
                else if ((x ^= (buffer[pos++] << 21)) < 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21);
                }
                else if ((x ^= ((long)buffer[pos++] << 28)) >= 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21) ^ (~0L << 28);
                }
                else if ((x ^= ((long)buffer[pos++] << 35)) < 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21) ^ (~0L << 28) ^ (~0L << 35);
                }
                else if ((x ^= ((long)buffer[pos++] << 42)) >= 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21) ^ (~0L << 28) ^ (~0L << 35) ^ (~0L << 42);
                }
                else if ((x ^= ((long)buffer[pos++] << 49)) < 0L)
                {
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21) ^ (~0L << 28) ^ (~0L << 35) ^ (~0L << 42)
                            ^ (~0L << 49);
                }
                else
                {
                    x ^= ((long)buffer[pos++] << 56);
                    x ^= (~0L << 7) ^ (~0L << 14) ^ (~0L << 21) ^ (~0L << 28) ^ (~0L << 35) ^ (~0L << 42)
                            ^ (~0L << 49) ^ (~0L << 56);
                    if (x < 0L)
                    {
                        if (buffer[pos] < 0L)
                        {
                            goto fastpath;  // Will throw malformedVarint()
                        }
                    }
                }
                return x;
            }
            return readVar64SlowPath(buffer, offset);
        }

        /** Variant of readRawVarint64 for when uncomfortably close to the limit. */
        /* Visible for testing */
        static long readVar64SlowPath(byte[] buffer, int offset)
        {

            long result = 0;
            for (int shift = 0; shift < 64; shift += 7)
            {
                var b = buffer[offset++];
                result |= (long)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }
            }
            throw new InvalidOperationException("invalid varLong. start offset:" + offset + " readOffset:" + offset);
        }

        public static short bytesToShort(byte byte1, byte byte2)
        {
            return (short)(((byte1 & 0xff) << 8) | ((byte2 & 0xff)));
        }


        public static int writeLong(long value, byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + LONG_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 8));
            }
            buf[offset++] = (byte)(value >> 56);
            buf[offset++] = (byte)(value >> 48);
            buf[offset++] = (byte)(value >> 40);
            buf[offset++] = (byte)(value >> 32);
            buf[offset++] = (byte)(value >> 24);
            buf[offset++] = (byte)(value >> 16);
            buf[offset++] = (byte)(value >> 8);
            buf[offset++] = (byte)(value);
            return offset;
        }


        public static int writeShort(short value, byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + SHORT_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 2));
            }
            buf[offset++] = (byte)(value >> 8);
            buf[offset++] = (byte)(value);
            return offset;
        }

        public static int writeInt(int value, byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (buf.Length < offset + INT_BYTE_LENGTH)
            {
                throw new IndexOutOfRangeException("buf.length is too small. buf.length:" + buf.Length + " offset:" + (offset + 4));
            }
            buf[offset++] = (byte)(value >> 24);
            buf[offset++] = (byte)(value >> 16);
            buf[offset++] = (byte)(value >> 8);
            buf[offset++] = (byte)(value);
            return offset;
        }

        public static int writeSVar32(int value, byte[] buf, int offset)
        {
            return writeVar32(intToZigZag(value), buf, offset);
        }

        public static int writeVar32(int value, byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            checkBound(buf.Length, offset);
            while (true)
            {
                if ((value & ~0x7F) == 0)
                {
                    buf[offset++] = (byte)value;
                    return offset;
                }
                else
                {
                    buf[offset++] = (byte)((value & 0x7F) | 0x80);
                    value >>= 7;
                }
            }
        }

        public static int shortToUnsignedShort(short value)
        {
            return value & 0xffff;
        }

        public static byte[] intToSVar32(int value)
        {
            return intToVar32(intToZigZag(value));
        }

        public static byte[] intToVar32(int value)
        {
            var bufferSize = BytesUtils.computeVar32Size(value);
            var buffer = new byte[bufferSize];
            writeVar64(value, buffer, 0);
            return buffer;
        }

        /**
         * copy google protocol buffer
         * https://github.com/google/protobuf/blob/master/java/src/main/java/com/google/protobuf/CodedOutputStream.java
         */
        public static int computeVar32Size(int value)
        {
            if ((value & (0xffffffff << 7)) == 0) return 1;
            if ((value & (0xffffffff << 14)) == 0) return 2;
            if ((value & (0xffffffff << 21)) == 0) return 3;
            if ((value & (0xffffffff << 28)) == 0) return 4;
            return 5;
        }


        public static int writeSVar64(int value, byte[] buf, int offset)
        {
            return writeVar64(longToZigZag(value), buf, offset);
        }

        /**
         * copy google protocol buffer
         * https://github.com/google/protobuf/blob/master/java/src/main/java/com/google/protobuf/CodedOutputStream.java
         */
        public static int writeVar64(long value, byte[] buf, int offset)
        {
            if (buf == null)
            {
                throw new NullReferenceException("buf must not be null");
            }
            checkBound(buf.Length, offset);

            while (true)
            {
                if ((value & ~0x7FL) == 0)
                {
                    buf[offset++] = (byte)value;
                    return offset;
                }
                else
                {
                    buf[offset++] = (byte)(((int)value & 0x7F) | 0x80);
                    value >>= 7;
                }
            }
        }

        static void checkBound(int bufferLength, int offset)
        {
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (offset >= bufferLength)
            {
                throw new IndexOutOfRangeException("invalid offset:" + offset + " bufferLength:" + bufferLength);
            }
        }

        public static byte[] longToSVar64(long value)
        {
            return longToVar64(longToZigZag(value));
        }

        public static byte[] longToVar64(long value)
        {
            int bufferSize = BytesUtils.computeVar64Size(value);
            byte[] buffer = new byte[bufferSize];
            writeVar64(value, buffer, 0);
            return buffer;
        }

        /**
         * copy google protocol buffer
         * https://github.com/google/protobuf/blob/master/java/src/main/java/com/google/protobuf/CodedOutputStream.java
         */
        public static int computeVar64Size(long value)
        {
            if (((ulong)value & (0xffffffffffffffffL << 7)) == 0) return 1;
            if (((ulong)value & (0xffffffffffffffffL << 14)) == 0) return 2;
            if (((ulong)value & (0xffffffffffffffffL << 21)) == 0) return 3;
            if (((ulong)value & (0xffffffffffffffffL << 28)) == 0) return 4;
            if (((ulong)value & (0xffffffffffffffffL << 35)) == 0) return 5;
            if (((ulong)value & (0xffffffffffffffffL << 42)) == 0) return 6;
            if (((ulong)value & (0xffffffffffffffffL << 49)) == 0) return 7;
            if (((ulong)value & (0xffffffffffffffffL << 56)) == 0) return 8;
            if (((ulong)value & (0xffffffffffffffffL << 63)) == 0) return 9;
            return 10;
        }


        private static int writeFirstLong0(long value, byte[] buf, int offset)
        {
            buf[offset] = (byte)(value >> 56);
            buf[1 + offset] = (byte)(value >> 48);
            buf[2 + offset] = (byte)(value >> 40);
            buf[3 + offset] = (byte)(value >> 32);
            buf[4 + offset] = (byte)(value >> 24);
            buf[5 + offset] = (byte)(value >> 16);
            buf[6 + offset] = (byte)(value >> 8);
            buf[7 + offset] = (byte)(value);
            return offset;
        }


        private static int writeSecondLong0(long value, byte[] buf, int offset)
        {
            buf[8 + offset] = (byte)(value >> 56);
            buf[9 + offset] = (byte)(value >> 48);
            buf[10 + offset] = (byte)(value >> 40);
            buf[11 + offset] = (byte)(value >> 32);
            buf[12 + offset] = (byte)(value >> 24);
            buf[13 + offset] = (byte)(value >> 16);
            buf[14 + offset] = (byte)(value >> 8);
            buf[15 + offset] = (byte)(value);
            return offset;
        }

        public static byte[] add(string prefix, long postfix)
        {
            if (prefix == null)
            {
                throw new NullReferenceException("prefix must not be null");
            }
            byte[] agentByte = toBytes(prefix);
            return add(agentByte, postfix);
        }

        public static byte[] add(byte[] preFix, long postfix)
        {
            byte[] buf = new byte[preFix.Length + LONG_BYTE_LENGTH];
            Array.Copy(preFix, 0, buf, 0, preFix.Length);
            writeLong(postfix, buf, preFix.Length);
            return buf;
        }

        public static byte[] add(byte[] preFix, short postfix)
        {
            if (preFix == null)
            {
                throw new NullReferenceException("preFix must not be null");
            }
            byte[] buf = new byte[preFix.Length + SHORT_BYTE_LENGTH];
            Array.Copy(preFix, 0, buf, 0, preFix.Length);
            writeShort(postfix, buf, preFix.Length);
            return buf;
        }

        public static byte[] add(byte[] preFix, int postfix)
        {
            if (preFix == null)
            {
                throw new NullReferenceException("preFix must not be null");
            }
            byte[] buf = new byte[preFix.Length + INT_BYTE_LENGTH];
            Array.Copy(preFix, 0, buf, 0, preFix.Length);
            writeInt(postfix, buf, preFix.Length);
            return buf;
        }

        public static byte[] add(int preFix, short postFix)
        {
            byte[] buf = new byte[INT_BYTE_LENGTH + SHORT_BYTE_LENGTH];
            writeInt(preFix, buf, 0);
            writeShort(postFix, buf, 4);
            return buf;
        }


        public static byte[] add(long preFix, short postFix)
        {
            byte[] buf = new byte[LONG_BYTE_LENGTH + SHORT_BYTE_LENGTH];
            writeLong(preFix, buf, 0);
            writeShort(postFix, buf, 8);
            return buf;
        }

        public static byte[] add(long preFix, short postFix, int intArg, short shortArg)
        {
            byte[] buf = new byte[LONG_BYTE_LENGTH + SHORT_BYTE_LENGTH + INT_BYTE_LENGTH + SHORT_BYTE_LENGTH];
            int offset = 0;
            writeLong(preFix, buf, offset);
            offset += LONG_BYTE_LENGTH;
            writeShort(postFix, buf, offset);
            offset += SHORT_BYTE_LENGTH;
            writeInt(intArg, buf, offset);
            offset += INT_BYTE_LENGTH;
            writeShort(shortArg, buf, offset);
            return buf;
        }


        public static byte[] toBytes(String value)
        {
            if (value == null)
            {
                return null;
            }
            return UTF8_CHARSET.GetBytes(value);
        }

        public static byte[] merge(byte[] b1, byte[] b2)
        {
            if (b1 == null)
            {
                throw new NullReferenceException("b1 must not be null");
            }
            if (b2 == null)
            {
                throw new NullReferenceException("b2 must not be null");
            }
            byte[] result = new byte[b1.Length + b2.Length];

            Array.Copy(b1, 0, result, 0, b1.Length);
            Array.Copy(b2, 0, result, b1.Length, b2.Length);

            return result;
        }

        public static byte[] toFixedLengthBytes(string str, int length)
        {
            if (length < 0)
            {
                throw new IndexOutOfRangeException("negative length:" + length);
            }
            byte[] b1 = toBytes(str);
            if (b1 == null)
            {
                return new byte[length];
            }

            if (b1.Length > length)
            {
                throw new IndexOutOfRangeException("String is longer then target length of bytes.");
            }
            byte[] b = new byte[length];
            Array.Copy(b1, 0, b, 0, b1.Length);

            return b;
        }


        public static int intToZigZag(int n)
        {
            return (n << 1) ^ (n >> 31);
        }

        public static int zigzagToInt(int n)
        {
            return (n >> 1) ^ -(n & 1);
        }


        public static long longToZigZag(long n)
        {
            return (n << 1) ^ (n >> 63);
        }

        public static long zigzagToLong(long n)
        {
            return (n >> 1) ^ -(n & 1);
        }

        public static byte[] concat(params byte[][] arrays)
        {
            int totalLength = 0;

            int length = arrays.Length;
            for (int i = 0; i < length; i++)
            {
                totalLength += arrays[i].Length;
            }

            byte[] result = new byte[totalLength];

            int currentIndex = 0;
            for (int i = 0; i < length; i++)
            {
                Array.Copy(arrays[i], 0, result, currentIndex, arrays[i].Length);
                currentIndex += arrays[i].Length;
            }

            return result;
        }

        public static String safeTrim(string str)
        {
            if (str == null)
            {
                return null;
            }
            return str.Trim();
        }

        public static String toString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return toString(bytes, 0, bytes.Length);
        }

        public static String toString(byte[] bytes, int offset, int length)
        {
            if (bytes == null)
            {
                return null;
            }
            if (offset < 0)
            {
                throw new IndexOutOfRangeException("negative offset:" + offset);
            }
            if (length == 0)
            {
                return "";
            }
            return UTF8_CHARSET.GetString(bytes, offset, length);
        }

        public static String toStringAndRightTrim(byte[] bytes, int offset, int length)
        {
            String str = toString(bytes, offset, length);
            return trimRight(str);
        }

        public static String trimRight(string str)
        {
            if (str == null)
            {
                return null;
            }
            int length = str.Length;
            int index = length;

            // need to use Character.isWhitespace()? may not needed.
            while (str[index - 1] <= ' ')
            {
                index--;
            }
            if (index == length)
            {
                return str;
            }
            else
            {
                return str.Substring(0, index);
            }
        }
    }
}
