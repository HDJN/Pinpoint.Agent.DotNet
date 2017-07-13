using Pinpoint.Agent.Thrift.Dto;

namespace Pinpoint.Agent.Context
{
    public class TSpanEventWrapper
    {
        public TSpanEvent SpanEvent { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }
    }
}
