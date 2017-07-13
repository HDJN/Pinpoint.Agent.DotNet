namespace Pinpoint.Agent.Common
{
    public class PacketType
    {
        public const short APPLICATION_SEND = 1;
        public const short APPLICATION_TRACE_SEND = 2;
        public const short APPLICATION_TRACE_SEND_ACK = 3;

        public const short APPLICATION_REQUEST = 5;
        public const short APPLICATION_RESPONSE = 6;


        public const short APPLICATION_STREAM_CREATE = 10;
        public const short APPLICATION_STREAM_CREATE_SUCCESS = 12;
        public const short APPLICATION_STREAM_CREATE_FAIL = 14;

        public const short APPLICATION_STREAM_CLOSE = 15;

        public const short APPLICATION_STREAM_PING = 17;
        public const short APPLICATION_STREAM_PONG = 18;

        public const short APPLICATION_STREAM_RESPONSE = 20;


        public const short CONTROL_CLIENT_CLOSE = 100;
        public const short CONTROL_SERVER_CLOSE = 110;

        // control packet
        public const short CONTROL_HANDSHAKE = 150;
        public const short CONTROL_HANDSHAKE_RESPONSE = 151;

        // keep stay because of performance in case of ping and pong. others removed.
        public const short CONTROL_PING = 200;
        public const short CONTROL_PONG = 201;

        public const short UNKNOWN = 500;
    }
}
