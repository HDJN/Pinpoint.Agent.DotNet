namespace Pinpoint.Agent.Common.Buffer
{
    using System;

    public class AutomaticBuffer : FixedBuffer
    {
        public AutomaticBuffer() : this(32)
        {

        }

        public AutomaticBuffer(int size) : base(size)
        {

        }

        public AutomaticBuffer(byte[] buffer) : base(buffer)
        {

        }


        protected void checkExpand(int size)
        {
            int remain = remaining();
            if (remain >= size)
            {
                return;
            }
            int length = buffer.Length;
            if (length == 0)
            {
                length = 1;
            }

            // after compute the buffer size, allocate it once for ado.
            int expandedBufferSize = computeExpandedBufferSize(size, length, remain);
            // allocate buffer
            byte[] expandedBuffer = new byte[expandedBufferSize];
            Array.Copy(buffer, 0, expandedBuffer, 0, buffer.Length);
            buffer = expandedBuffer;
        }

        protected int computeExpandedBufferSize(int size, int length, int remain)
        {
            int expandedBufferSize = 0;
            while (remain < size)
            {
                length <<= 1;
                expandedBufferSize = length;
                remain = expandedBufferSize - offset;
            }
            return expandedBufferSize;
        }

        public void putPadBytes(byte[] bytes, int totalLength)
        {
            checkExpand(totalLength);
            base.putPadBytes(bytes, totalLength);
        }


        public void putPrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                checkExpand(1);
                base.putSVInt(NULL);
            }
            else
            {
                checkExpand(bytes.Length + BytesUtils.VINT_MAX_SIZE);
                base.putSVInt(bytes.Length);
                base.putBytes(bytes);
            }
        }

        public void put2PrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                checkExpand(BytesUtils.SHORT_BYTE_LENGTH);
                base.putShort((short)NULL);
            }
            else
            {
                if (bytes.Length > Int16.MaxValue)
                {
                    throw new IndexOutOfRangeException("too large bytes length:" + bytes.Length);
                }
                checkExpand(bytes.Length + BytesUtils.SHORT_BYTE_LENGTH);
                base.putShort((short)bytes.Length);
                base.putBytes(bytes);
            }
        }

        public void put4PrefixedBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                checkExpand(BytesUtils.INT_BYTE_LENGTH);
                base.putInt(NULL);
            }
            else
            {
                checkExpand(bytes.Length + BytesUtils.INT_BYTE_LENGTH);
                base.putInt(bytes.Length);
                base.putBytes(bytes);
            }
        }

        public void putPadString(String str, int totalLength)
        {
            checkExpand(totalLength);
            base.putPadString(str, totalLength);
        }


        public void putPrefixedString(String str)
        {
            byte[] bytes = BytesUtils.toBytes(str);
            this.putPrefixedBytes(bytes);
        }

        public void put2PrefixedString(String str)
        {
            byte[] bytes = BytesUtils.toBytes(str);
            this.put2PrefixedBytes(bytes);
        }

        public void put4PrefixedString(String str)
        {
            byte[] bytes = BytesUtils.toBytes(str);
            this.put4PrefixedBytes(bytes);
        }

        public void putByte(byte v)
        {
            checkExpand(1);
            base.putByte(v);
        }

        public void put(byte v)
        {
            putByte(v);
        }

        public void putBoolean(bool v)
        {
            checkExpand(1);
            base.putBoolean(v);
        }

        public void put(bool v)
        {
            putBoolean(v);
        }

        public void putShort(short v)
        {
            checkExpand(2);
            base.putShort(v);
        }



        public void put(short v)
        {
            putShort(v);
        }


        public void putInt(int v)
        {
            checkExpand(4);
            base.putInt(v);
        }



        public void put(int v)
        {
            putInt(v);
        }


        public void putVInt(int v)
        {
            checkExpand(BytesUtils.VLONG_MAX_SIZE);
            base.putVInt(v);
        }



        public void putVar(int v)
        {
            putVInt(v);
        }


        public void putSVInt(int v)
        {
            checkExpand(BytesUtils.VINT_MAX_SIZE);
            base.putSVInt(v);
        }



        public void putSVar(int v)
        {
            putSVInt(v);
        }



        public void putVLong(long v)
        {
            checkExpand(BytesUtils.VLONG_MAX_SIZE);
            base.putVLong(v);
        }



        public void putVar(long v)
        {
            putVLong(v);
        }


        public void putSVLong(long v)
        {
            checkExpand(BytesUtils.VLONG_MAX_SIZE);
            base.putSVLong(v);
        }



        public void putSVar(long v)
        {
            putSVLong(v);
        }


        public void putLong(long v)
        {
            checkExpand(8);
            base.putLong(v);
        }



        public void put(long v)
        {
            putLong(v);
        }



        public void putBytes(byte[] v)
        {
            if (v == null)
            {
                throw new NullReferenceException("v must not be null");
            }
            checkExpand(v.Length);
            base.putBytes(v);
        }


        public void put(byte[] v)
        {
            putBytes(v);
        }
    }
}
