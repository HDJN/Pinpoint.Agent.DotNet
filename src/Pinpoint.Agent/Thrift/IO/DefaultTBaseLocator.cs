namespace Pinpoint.Agent.Thrift.IO
{
    using Agent.Thrift.Dto;
    using global::Thrift;
    using global::Thrift.Protocol;
    using System;

    public class DefaultTBaseLocator : TBaseLocator
    {
        private const short NETWORK_CHECK = 10;

        private readonly Header NETWORK_CHECK_HEADER = createHeader(NETWORK_CHECK);

        private const short SPAN = 40;
        private readonly Header SPAN_HEADER = createHeader(SPAN);

        private const short AGENT_INFO = 50;
        private readonly Header AGENT_INFO_HEADER = createHeader(AGENT_INFO);

        private const short AGENT_STAT = 55;
        private readonly Header AGENT_STAT_HEADER = createHeader(AGENT_STAT);
        private const short AGENT_STAT_BATCH = 56;
        private readonly Header AGENT_STAT_BATCH_HEADER = createHeader(AGENT_STAT_BATCH);

        private const short SPANCHUNK = 70;
        private readonly Header SPANCHUNK_HEADER = createHeader(SPANCHUNK);

        private const short SPANEVENT = 80;
        private readonly Header SPANEVENT_HEADER = createHeader(SPANEVENT);

        private const short SQLMETADATA = 300;
        private readonly Header SQLMETADATA_HEADER = createHeader(SQLMETADATA);

        private const short APIMETADATA = 310;
        private readonly Header APIMETADATA_HEADER = createHeader(APIMETADATA);

        private const short RESULT = 320;
        private readonly Header RESULT_HEADER = createHeader(RESULT);

        private const short STRINGMETADATA = 330;
        private readonly Header STRINGMETADATA_HEADER = createHeader(STRINGMETADATA);

        private const short CHUNK = 400;
        private readonly Header CHUNK_HEADER = createHeader(CHUNK);

        public TBase TBaseLookup(short type)
        {
            switch (type)
            {
                case SPAN:
                    return new TSpan();
                case AGENT_INFO:
                    return new TAgentInfo();
                case AGENT_STAT:
                    return new TAgentStat();
                case AGENT_STAT_BATCH:
                    return new TAgentStatBatch();
                case SPANCHUNK:
                    return new TSpanChunk();
                case SPANEVENT:
                    return new TSpanEvent();
                case SQLMETADATA:
                    return new TSqlMetaData();
                case APIMETADATA:
                    return new TApiMetaData();
                case RESULT:
                    return new TResult();
                case STRINGMETADATA:
                    return new TStringMetaData();
                    //case NETWORK_CHECK:
                    //    return new NetworkAvailabilityCheckPacket();
            }
            throw new TException("Unsupported type:" + type);
        }

        public Header HeaderLookup(TBase tbase)
        {
            if (tbase == null)
            {
                throw new Exception("tbase must not be null");
            }
            if (tbase is TSpan)
            {
                return SPAN_HEADER;
            }
            if (tbase is TSpanChunk)
            {
                return SPANCHUNK_HEADER;
            }
            if (tbase is TSpanEvent)
            {
                return SPANEVENT_HEADER;
            }
            if (tbase is TAgentInfo)
            {
                return AGENT_INFO_HEADER;
            }
            if (tbase is TAgentStat)
            {
                return AGENT_STAT_HEADER;
            }
            if (tbase is TAgentStatBatch)
            {
                return AGENT_STAT_BATCH_HEADER;
            }
            if (tbase is TSqlMetaData)
            {
                return SQLMETADATA_HEADER;
            }
            if (tbase is TApiMetaData)
            {
                return APIMETADATA_HEADER;
            }
            if (tbase is TResult)
            {
                return RESULT_HEADER;
            }
            if (tbase is TStringMetaData)
            {
                return STRINGMETADATA_HEADER;
            }
            //if (tbase is NetworkAvailabilityCheckPacket) {
            //    return NETWORK_CHECK_HEADER;
            //}

            throw new TException("Unsupported Type" + tbase.GetType());
        }

        public bool IsSupport(short type)
        {
            try
            {
                TBaseLookup(type);
                return true;
            }
            catch (TException ignore)
            {
                // skip
            }

            return false;
        }

        private static Header createHeader(short type)
        {
            Header header = new Header();
            header.Type = type;
            return header;
        }

        public Header getChunkHeader()
        {
            return CHUNK_HEADER;
        }

        public bool isChunkHeader(short type)
        {
            return CHUNK == type;
        }
    }
}
