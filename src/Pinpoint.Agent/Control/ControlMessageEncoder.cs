namespace Pinpoint.Agent.Pinpoint
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;


    public class ControlMessageEncoder
    {
        public byte[] EncodeMap(Dictionary<string, Object> value)
        {
            var @out = new List<Byte>();
            @out.Add((byte)ControlMessageProtocolConstant.CONTROL_CHARACTER_MAP_START);
            foreach (var entry in value)
            {
                Encode(entry.Key, @out);
                Encode(entry.Value, @out);
            }
            @out.Add((byte)ControlMessageProtocolConstant.CONTROL_CHARACTER_MAP_END);
            return @out.ToArray();
        }

        private void Encode(Object value, List<Byte> @out)
        {
            if (value == null)
            {
                EncodeNull(@out);
            }
            else if (value is String)
            {
                EncodeString((String)value, @out);
            }
            else if (value is bool)
            {
                EncodeBoolean((Boolean)value, @out);
            }
            else if (value is short)
            {
                EncodeInt((short)value, @out);
            }
            else if (value is int)
            {
                EncodeInt((int)value, @out);
            }
            else if (value is long)
            {
                EncodeLong((long)value, @out);
            }
            else if (value is float)
            {
                EncodeDouble(((float)value), @out);
            }
            else if (value is double)
            {
                EncodeDouble((double)value, @out);
            }
            else if (value is IList)
            {
                EncodeCollection((IList)value, @out);
            }
            else
            {
                throw new Exception("Unsupported type : " + value.GetType().Name);
            }
        }

        private void EncodeNull(List<Byte> @out)
        {
            @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_NULL);
        }

        private void EncodeString(String value, List<Byte> @out)
        {
            @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_STRING);
            PutPrefixedBytes(Encoding.UTF8.GetBytes(value), @out);
        }

        private void EncodeBoolean(bool value, List<Byte> @out)
        {
            if (value)
            {
                @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_BOOL_TRUE);
            }
            else
            {
                @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_BOOL_FALSE);
            }
        }

        private void EncodeInt(int value, List<Byte> @out)
        {
            @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_INT);

            @out.Add((byte)(value >> 24));
            @out.Add((byte)(value >> 16));
            @out.Add((byte)(value >> 8));
            @out.Add((byte)(value));
        }

        private void EncodeLong(long value, List<Byte> @out)
        {
            @out.Add((byte)ControlMessageProtocolConstant.TYPE_CHARACTER_LONG);

            @out.Add((byte)(value >> 56));
            @out.Add((byte)(value >> 48));
            @out.Add((byte)(value >> 40));
            @out.Add((byte)(value >> 32));
            @out.Add((byte)(value >> 24));
            @out.Add((byte)(value >> 16));
            @out.Add((byte)(value >> 8));
            @out.Add((byte)(value));
        }

        private void EncodeDouble(double value, List<Byte> @out)
        {
            throw new NotImplementedException();
        }

        private void EncodeCollection(IList collection, List<Byte> @out)
        {
            @out.Add((byte)ControlMessageProtocolConstant.CONTROL_CHARACTER_LIST_START);
            foreach (Object element in collection)
            {
                Encode(element, @out);
            }
            @out.Add((byte)ControlMessageProtocolConstant.CONTROL_CHARACTER_LIST_END);
        }

        private void PutPrefixedBytes(byte[] value, List<Byte> @out)
        {
            int length = value.Length;

            byte[] lengthBuf = new byte[5];

            int idx = 0;
            while (true)
            {
                if ((length & 0xFFFFFF80) == 0)
                {
                    lengthBuf[(idx++)] = (byte)length;
                    break;
                }

                lengthBuf[(idx++)] = (byte)(length & 0x7F | 0x80);

                length >>= 7;
            }

            for (int i = 0; i < idx; i++)
            {
                @out.Add(lengthBuf[i]);
            }

            @out.AddRange(value);
        }
    }
}
