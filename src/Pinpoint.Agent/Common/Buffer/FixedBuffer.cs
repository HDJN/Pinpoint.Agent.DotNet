namespace Pinpoint.Agent.Common.Buffer
{
    using System;
    using System.IO;
    using System.Text;

    public class FixedBuffer : Buffer
    {
        protected static readonly int NULL = -1;
        protected byte[] buffer;
        protected int offset;

        int BOOLEAN_FALSE { get { return 0; } }

        int BOOLEAN_TRUE { get { return 1; } }

        byte[] EMPTY { get { return new byte[0]; } }

        Encoding UTF8_CHARSET { get { return Encoding.UTF8; } }

        public FixedBuffer() : this(32)
        {

        }

        public FixedBuffer(int bufferSize)
        {
            if (bufferSize < 0)
            {
                throw new IndexOutOfRangeException("negative bufferSize:" + bufferSize);
            }
            this.buffer = new byte[bufferSize];
            this.offset = 0;
        }

        public FixedBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new NullReferenceException("buffer must not be null");
            }
            this.buffer = buffer;
            this.offset = 0;
        }

        public void putPadBytes(byte[] bytes, int totalLength)
        {
            if (bytes == null)
            {
                bytes = EMPTY;
            }
            if (bytes.Length > totalLength)
            {
                throw new IndexOutOfRangeException("bytes too big:" + bytes.Length + " totalLength:" + totalLength);
            }
            putBytes(bytes);
            int padSize = totalLength - bytes.Length;
            if (padSize > 0)
            {
                putPad(padSize);
            }
        }

        private void putPad(int padSize)
        {
            for (int i = 0; i < padSize; i++)
            {
                putByte((byte)0);
            }
        }


        public void putPrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                putSVInt(NULL);
            }
            else
            {
                putSVInt(bytes.Length);
                putBytes(bytes);
            }
        }

        public void put2PrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                putShort((short)NULL);
            }
            else
            {
                if (bytes.Length > Int16.MaxValue)
                {
                    throw new IndexOutOfRangeException("too large bytes length:" + bytes.Length);
                }
                putShort((short)bytes.Length);
                putBytes(bytes);
            }
        }

        public void put4PrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                putInt(NULL);
            }
            else
            {
                putInt(bytes.Length);
                putBytes(bytes);
            }
        }

        public void putPadString(String str, int totalLength)
        {
            var bytes = BytesUtils.toBytes(str);
            putPadBytes(bytes, totalLength);
        }

        public void putPrefixedString(String str)
        {
            var bytes = BytesUtils.toBytes(str);
            putPrefixedBytes(bytes);
        }

        public void put2PrefixedString(String str)
        {
            var bytes = BytesUtils.toBytes(str);
            if (bytes == null)
            {
                putShort((short)NULL);
                return;
            }
            if (bytes.Length > Int16.MaxValue)
            {
                throw new IndexOutOfRangeException("too large String size:" + bytes.Length);
            }
            put2PrefixedBytes(bytes);
        }

        public void put4PrefixedString(String str)
        {
            var bytes = BytesUtils.toBytes(str);
            if (bytes == null)
            {
                putInt(NULL);
                return;
            }
            put4PrefixedBytes(bytes);
        }

        public void putByte(byte v)
        {
            this.buffer[offset++] = v;
        }

        public void put(byte v)
        {
            putByte(v);
        }

        public void putBoolean(bool v)
        {
            if (v)
            {
                this.buffer[offset++] = (byte)BOOLEAN_TRUE;
            }
            else
            {
                this.buffer[offset++] = (byte)BOOLEAN_FALSE;
            }
        }

        public void put(bool v)
        {
            putBoolean(v);
        }

        public void putInt(int v)
        {
            this.offset = BytesUtils.writeInt(v, buffer, offset);
        }


        public void put(int v)
        {
            putInt(v);
        }


        public void putVInt(int v)
        {
            if (v >= 0)
            {
                putVar32(v);
            }
            else
            {
                putVar64((long)v);
            }
        }

        public void putVar(int v)
        {
            putVInt(v);
        }

        public void putSVInt(int v)
        {
            this.offset = BytesUtils.writeSVar32(v, buffer, offset);
        }

        public void putSVar(int v)
        {
            putSVInt(v);
        }

        private void putVar32(int v)
        {
            this.offset = BytesUtils.writeVar32(v, buffer, offset);
        }

        public void putShort(short v)
        {
            this.offset = BytesUtils.writeShort(v, buffer, offset);
        }

        public void put(short v)
        {
            putShort(v);
        }

        public void putLong(long v)
        {
            this.offset = BytesUtils.writeLong(v, buffer, offset);
        }

        public void put(long v)
        {
            putLong(v);
        }

        public void putVLong(long v)
        {
            putVar64(v);
        }

        public void putVar(long v)
        {
            putVLong(v);
        }

        public void putSVLong(long v)
        {
            putVar64(BytesUtils.longToZigZag(v));
        }

        public void putSVar(long v)
        {
            putSVLong(v);
        }

        private void putVar64(long v)
        {
            this.offset = BytesUtils.writeVar64(v, buffer, offset);
        }

        public void putDouble(double v)
        {
            throw new NotImplementedException();
        }

        public void put(double v)
        {
            putDouble(v);
        }

        public void putVDouble(double v)
        {
            throw new NotImplementedException();
        }


        public void putVar(double v)
        {
            putVDouble(v);
        }

        public void putSVDouble(double v)
        {
            throw new NotImplementedException();
        }

        public void putSVar(double v)
        {
            putSVDouble(v);
        }

        public void putBytes(byte[] v)
        {
            if (v == null)
            {
                throw new NullReferenceException("v must not be null");
            }
            Array.Copy(v, 0, buffer, offset, v.Length);
            this.offset = offset + v.Length;
        }

        public void put(byte[] v)
        {
            putBytes(v);
        }

        public byte getByte(int index)
        {
            return this.buffer[offset];
        }


        public byte readByte()
        {
            return this.buffer[offset++];
        }


        public int readUnsignedByte()
        {
            return readByte() & 0xff;
        }


        public bool readBoolean()
        {
            byte b = readByte();
            return b == BOOLEAN_TRUE;
        }


        public int readInt()
        {
            int i = BytesUtils.bytesToInt(buffer, offset);
            this.offset = this.offset + 4;
            return i;
        }


        public int readVInt()
        {
            // borrowing the protocol buffer's concept of variable-length encoding
            // copy https://github.com/google/protobuf 2.6.1
            // CodedInputStream.java -> int readRawVarint32()

            // See implementation notes for readRawVarint64
            fastpath:
            {
                int pos = this.offset;
                int bufferSize = this.buffer.Length;
                if (bufferSize == pos)
                {
                    goto fastpath;
                }

                byte[] buffer = this.buffer;
                int x;
                if ((x = buffer[pos++]) >= 0)
                {
                    this.offset = pos;
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
                            buffer[pos++] < 0)
                    {
                        goto fastpath;  // Will throw malformedVarint()
                    }
                }
                this.offset = pos;
                return x;
            }
            return (int)readVar64SlowPath();
        }



        public int readVarInt()
        {
            return readVInt();
        }

        /** Variant of readRawVarint64 for when uncomfortably close to the limit. */
        /* Visible for testing */
        long readVar64SlowPath()
        {
            int copyOffset = this.offset;
            long result = 0;
            for (int shift = 0; shift < 64; shift += 7)
            {
                byte b = this.buffer[copyOffset++];
                result |= (long)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    this.offset = copyOffset;
                    return result;
                }
            }
            throw new Exception("invalid varLong. start offset:" + this.offset + " readOffset:" + offset);
        }


        public int readSVInt()
        {
            return BytesUtils.zigzagToInt(readVInt());
        }


        public int readSVarInt()
        {
            return readSVInt();
        }


        public short readShort()
        {
            var i = BytesUtils.bytesToShort(buffer, offset);
            this.offset = this.offset + 2;
            return i;
        }

        public int readUnsignedShort()
        {
            return readShort() & 0xFFFF;
        }


        public long readLong()
        {
            long l = BytesUtils.bytesToLong(buffer, offset);
            this.offset = this.offset + 8;
            return l;
        }


        public long readVLong()
        {
            // borrowing the protocol buffer's concept of variable-length encoding
            // copy https://github.com/google/protobuf 2.6.1
            // CodedInputStream.java -> long readRawVarint64() throws IOException

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
                int bufferSize = this.buffer.Length;
                if (bufferSize == pos)
                {
                    goto fastpath;
                }

                byte[] buffer = this.buffer;
                long x;
                int y;
                if ((y = buffer[pos++]) >= 0)
                {
                    this.offset = pos;
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
                        if (buffer[pos++] < 0L)
                        {
                            goto fastpath;  // Will throw malformedVarint()
                        }
                    }
                }
                this.offset = pos;
                return x;
            }
            return readVar64SlowPath();
        }



        public long readVarLong()
        {
            return readVLong();
        }


        public long readSVLong()
        {
            return BytesUtils.zigzagToLong(readVLong());
        }


        public long readSVarLong()
        {
            return readSVLong();
        }


        public double readDouble()
        {
            throw new NotImplementedException();
        }


        public double readVDouble()
        {
            throw new NotImplementedException();
        }



        public double readVarDouble()
        {
            return readVDouble();
        }


        public double readSVDouble()
        {
            throw new NotImplementedException();
        }



        public double readSVarDouble()
        {
            return readSVDouble();
        }


        public byte[] readPadBytes(int totalLength)
        {
            return readBytes(totalLength);
        }


        public String readPadString(int totalLength)
        {
            return readString(totalLength);
        }


        public String readPadStringAndRightTrim(int totalLength)
        {
            String str = BytesUtils.toStringAndRightTrim(buffer, offset, totalLength);
            this.offset = offset + totalLength;
            return str;
        }



        public byte[] readPrefixedBytes()
        {
            int size = readSVInt();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return EMPTY;
            }
            return readBytes(size);
        }


        public byte[] read2PrefixedBytes()
        {
            int size = readShort();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return EMPTY;
            }
            return readBytes(size);
        }


        public byte[] read4PrefixedBytes()
        {
            int size = readInt();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return EMPTY;
            }
            return readBytes(size);
        }


        private byte[] readBytes(int size)
        {
            byte[] b = new byte[size];
            Array.Copy(buffer, offset, b, 0, size);
            this.offset = offset + size;
            return b;
        }


        public String readPrefixedString()
        {
            int size = readSVInt();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return "";
            }
            return readString(size);
        }


        public String read2PrefixedString()
        {
            int size = readShort();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return "";
            }
            return readString(size);
        }


        public String read4PrefixedString()
        {
            int size = readInt();
            if (size == NULL)
            {
                return null;
            }
            if (size == 0)
            {
                return "";
            }
            return readString(size);
        }


        private String readString(int size)
        {
            String s = newString(size);
            this.offset = offset + size;
            return s;
        }

        private String newString(int size)
        {
            return UTF8_CHARSET.GetString(buffer, offset, size);
        }

        /**
         * Be careful that if internal buffer's length is as same as offset,
         * then just return internal buffer without copying memory for improving performance.
         * @return
         */

        public byte[] getBuffer()
        {
            if (offset == buffer.Length)
            {
                return this.buffer;
            }
            else
            {
                return copyBuffer();
            }
        }


        public byte[] copyBuffer()
        {
            byte[] copy = new byte[offset];
            Array.Copy(buffer, 0, copy, 0, offset);
            return copy;
        }


        public MemoryStream wrapByteBuffer()
        {
            return new MemoryStream(this.buffer, 0, offset);
        }

        /**
         * return internal buffer
         * @return
         */

        public byte[] getInternalBuffer()
        {
            return this.buffer;
        }


        public void setOffset(int offset)
        {
            this.offset = offset;
        }


        public int getOffset()
        {
            return offset;
        }

        /**
         *  Since 1.6.0. Use {@link Buffer#remaining()}
         */


        public int limit()
        {
            return remaining();
        }


        public int remaining()
        {
            return buffer.Length - offset;
        }


        public bool hasRemaining()
        {
            return offset < buffer.Length;
        }
    }
}
