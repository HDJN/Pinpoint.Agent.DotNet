namespace Pinpoint.Agent.Control
{
    using Common;
    using global::Thrift.Protocol;
    using Pinpoint;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class ControlMessageDecoder
    {
        private readonly Encoding charset = Encoding.UTF8;
        private object Integer;

        public ControlMessageDecoder()
        {

        }

        public Object Decode(byte[] @in)
        {
            return Decode(new MemoryStream(@in));
        }

        public Object Decode(MemoryStream @in)
        {
            int type = @in.ReadByte();
            switch (type)
            {
                case ControlMessageProtocolConstant.TYPE_CHARACTER_NULL:
                    return null;
                case ControlMessageProtocolConstant.TYPE_CHARACTER_BOOL_TRUE:
                    return true;
                case ControlMessageProtocolConstant.TYPE_CHARACTER_BOOL_FALSE:
                    return false;
                case ControlMessageProtocolConstant.TYPE_CHARACTER_INT:
                    return BufferHelper.ReadInt(@in);
                case ControlMessageProtocolConstant.TYPE_CHARACTER_LONG:
                    return BufferHelper.ReadLong(@in);
                case ControlMessageProtocolConstant.TYPE_CHARACTER_DOUBLE:
                    throw new NotImplementedException();
                case ControlMessageProtocolConstant.TYPE_CHARACTER_STRING:
                    return DecodeString(@in);
                case ControlMessageProtocolConstant.CONTROL_CHARACTER_LIST_START:
                    var answerList = new List<object>();
                    while (!IsListFinished(@in))
                    {
                        answerList.Add(Decode(@in));
                    }
                    @in.ReadByte(); // Skip the terminator
                    return answerList;
                case ControlMessageProtocolConstant.CONTROL_CHARACTER_MAP_START:
                    var answerMap = new Dictionary<object, object>();
                    while (!IsMapFinished(@in))
                    {
                        var key = Decode(@in);
                        var value = Decode(@in);
                        answerMap.Add(key, value);
                    }
                    @in.ReadByte(); // Skip the terminator
                    return answerMap;
                default:
                    throw new TProtocolException("invalid type character: " + (char)type + " (" + type + ")");
            }
        }

        private Object DecodeString(MemoryStream @in)
        {
            int length = ReadStringLength(@in);

            byte[] bytesToEncode = new byte[length];
            @in.Read(bytesToEncode, 0, length);

            return charset.GetString(bytesToEncode);
        }

        private bool IsMapFinished(MemoryStream @in)
        {
            var result = @in.ReadByte() == ControlMessageProtocolConstant.CONTROL_CHARACTER_MAP_END;
            @in.Seek(-1, SeekOrigin.Current);
            return result;
        }

        private bool IsListFinished(MemoryStream @in)
        {
            var result = @in.ReadByte() == ControlMessageProtocolConstant.CONTROL_CHARACTER_LIST_END;
            @in.Seek(-1, SeekOrigin.Current);
            return result;
        }

        private int ReadStringLength(MemoryStream @in)
        {
            int result = 0;
            int shift = 0;

            while (true)
            {
                byte b = (byte)@in.ReadByte();
                result |= (b & 0x7F) << shift;
                if ((b & 0x80) != 128)
                    break;
                shift += 7;
            }
            return result;
        }
    }
}
