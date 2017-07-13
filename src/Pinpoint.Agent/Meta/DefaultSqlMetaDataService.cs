namespace Pinpoint.Agent.Meta
{
    using Common;
    using Network;
    using Packet;
    using System;
    using Thrift.Dto;
    using Thrift.IO;

    public class DefaultSqlMetaDataService : ISqlMetaDataService
    {
        public String AgentId { get; set; }

        public long AgentStartTime { get; set; }

        private SimpleCache<String> apiCache = new SimpleCache<String>();

        private DefaultPinpointTcpClient enhancedDataSender { get; set; }

        public DefaultSqlMetaDataService(String agentId, long agentStartTime, DefaultPinpointTcpClient enhancedDataSender)
        {
            if (agentId == null)
            {
                throw new NullReferenceException("agentId must not be null");
            }
            if (enhancedDataSender == null)
            {
                throw new NullReferenceException("enhancedDataSender must not be null");
            }
            this.AgentId = agentId;
            this.AgentStartTime = agentStartTime;
            this.enhancedDataSender = enhancedDataSender;
        }

        public int CacheSql(DefaultParsingResult parsingResult)
        {
            var result = this.apiCache.put(parsingResult.Sql);

            parsingResult.Id = result.Id;

            if (result.NewValue)
            {
                var sqlMetaData = new TSqlMetaData();
                sqlMetaData.AgentId = AgentId;
                sqlMetaData.AgentStartTime = AgentStartTime;

                sqlMetaData.SqlId = parsingResult.Id;
                sqlMetaData.Sql = parsingResult.Sql;


                RequestPacket request = null;
                using (var serializer = new HeaderTBaseSerializer())
                {
                    request = new RequestPacket(IdGenerator.SequenceId(), serializer.serialize(sqlMetaData));
                    this.enhancedDataSender.Send(request.ToBuffer());
                }
            }

            return result.Id;
        }

        public DefaultParsingResult ParseSql(string sql)
        {
            return new DefaultParsingResult() { Sql = sql };
        }
    }
}
