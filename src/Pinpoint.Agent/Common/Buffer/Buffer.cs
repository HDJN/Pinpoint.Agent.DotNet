namespace Pinpoint.Agent.Common.Buffer
{
    using System;
    using System.IO;
    using System.Text;

    public interface Buffer
    {
        void putPadBytes(byte[] bytes, int totalLength);

        void putPrefixedBytes(byte[] bytes);

        void put2PrefixedBytes(byte[] bytes);

        void put4PrefixedBytes(byte[] bytes);

        void putPadString(String str, int totalLength);

        void putPrefixedString(String str);

        void put2PrefixedString(String str);

        void put4PrefixedString(String str);

        void putByte(byte v);

        void put(byte v);

        void putBoolean(bool v);

        void put(bool v);

        void putInt(int v);

        void put(int v);

        /**
         * put value using the variable-length encoding especially for constants
         * the size using variable-length encoding is bigger than using fixed-length int when v is negative.
         * if there are a lot of negative value in a buffer, it's very inefficient.
         * instead use putSVar in that case.
         * putVar compared to putSVar has a little benefit to use a less cpu due to no zigzag operation.
         * it has more benefit to use putSVar whenever v is negative.
         * consume 1~10 bytes ( integer's max value consumes 5 bytes, integer's min value consumes 10 bytes)
         * @param v
         */
        void putVInt(int v);

        void putVar(int v);

        /**
         * put value using variable-length encoding
         * useful for same distribution of constants and negatives value
         * consume 1~5 bytes ( integer's max value consumes 5 bytes, integer's min value consumes 5 bytes)

         * @param v
         */
        void putSVInt(int v);

        void putSVar(int v);

        void putShort(short v);

        void put(short v);

        void putLong(long v);

        void put(long v);

        /**
         * put value using the variable-length encoding especially for constants
         * the size using variable-length encoding is bigger than using fixed-length int when v is negative.
         * if there are a lot of negative value in a buffer, it's very inefficient.
         * instead use putSVar in that case.
         * @param v
         */
        void putVLong(long v);

        void putVar(long v);

        /**
         * put value using variable-length encoding
         * useful for same distribution of constants and negatives value
         * @param v
         */
        void putSVLong(long v);

        void putSVar(long v);

        void putDouble(double v);

        void put(double v);

        /**
         * put value using the variable-length encoding especially for constants
         * the size using variable-length encoding is bigger than using fixed-length int when v is negative.
         * if there are a lot of negative value in a buffer, it's very inefficient.
         * instead use putSVar in that case.
         * @param v
         */
        void putVDouble(double v);

        void putVar(double v);

        /**
         * put value using variable-length encoding
         * useful for same distribution of constants and negatives value
         * @param v
         */
        void putSVDouble(double v);

        void putSVar(double v);

        void putBytes(byte[] v);

        void put(byte[] v);

        byte getByte(int index);

        byte readByte();

        int readUnsignedByte();

        bool readBoolean();

        int readInt();

        int readVInt();

        int readVarInt();


        int readSVInt();

        int readSVarInt();


        short readShort();

        long readLong();

        long readVLong();

        long readVarLong();

        long readSVLong();

        long readSVarLong();

        double readDouble();

        double readVDouble();

        double readVarDouble();

        double readSVDouble();

        double readSVarDouble();

        byte[] readPadBytes(int totalLength);

        String readPadString(int totalLength);

        String readPadStringAndRightTrim(int totalLength);

        byte[] readPrefixedBytes();

        byte[] read2PrefixedBytes();

        byte[] read4PrefixedBytes();

        String readPrefixedString();

        String read2PrefixedString();

        String read4PrefixedString();

        byte[] getBuffer();

        byte[] copyBuffer();

        byte[] getInternalBuffer();

        MemoryStream wrapByteBuffer();

        void setOffset(int offset);

        int getOffset();

        int limit();

        int remaining();

        bool hasRemaining();
    }
}
