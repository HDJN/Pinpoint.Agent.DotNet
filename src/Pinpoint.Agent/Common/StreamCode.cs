namespace Pinpoint.Agent.Common
{
    public enum StreamCode
    {
        OK = 0,
        UNKNWON_ERROR = 100,
        ID_ERROR = 110,
        ID_ILLEGAL = 111,
        ID_DUPLICATED = 112,
        ID_NOT_FOUND = 113,
        STATE_ERROR = 120,
        STATE_NOT_CONNECTED = 121,
        STATE_CLOSED = 122,
        TYPE_ERROR = 130,
        TYPE_UNKNOWN = 131,
        TYPE_UNSUPPORT = 132,
        PACKET_ERROR = 140,
        PACKET_UNKNOWN = 141,
        PACKET_UNSUPPORT = 142,
        CONNECTION_ERRROR = 150,
        CONNECTION_NOT_FOUND = 151,
        CONNECTION_TIMEOUT = 152,
        CONNECTION_UNSUPPORT = 153,
        ROUTE_ERROR = 160
    }
}
