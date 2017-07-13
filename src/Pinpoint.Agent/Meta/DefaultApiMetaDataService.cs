namespace Pinpoint.Agent.Meta
{
    using Network;
    using Thrift.Dto;
    using Thrift.IO;
    using System;
    using Packet;
    using Common;

    public class DefaultApiMetaDataService : IApiMetaDataService
    {
        private SimpleCache<String> apiCache = new SimpleCache<String>();

        private String agentId;
        private long agentStartTime;
        private DefaultPinpointTcpClient enhancedDataSender;

        public DefaultApiMetaDataService(String agentId, long agentStartTime, DefaultPinpointTcpClient enhancedDataSender)
        {
            if (agentId == null)
            {
                throw new NullReferenceException("agentId must not be null");
            }
            if (enhancedDataSender == null)
            {
                throw new NullReferenceException("enhancedDataSender must not be null");
            }
            this.agentId = agentId;
            this.agentStartTime = agentStartTime;
            this.enhancedDataSender = enhancedDataSender;
        }

        public int CacheApi(MethodDescriptor methodDescriptor)
        {
            var fullName = methodDescriptor.GetFullName();
            var result = this.apiCache.put(fullName);

            methodDescriptor.ApiId = result.Id;

            if (result.NewValue)
            {
                var apiMetadata = new TApiMetaData();
                apiMetadata.AgentId = agentId;
                apiMetadata.AgentStartTime = agentStartTime;

                apiMetadata.ApiId = result.Id;
                apiMetadata.ApiInfo = methodDescriptor.ApiDescriptor;
                apiMetadata.Line = methodDescriptor.LineNumber;
                apiMetadata.Type = methodDescriptor.Type;

                RequestPacket request = null;
                using (var serializer = new HeaderTBaseSerializer())
                {
                    request = new RequestPacket(IdGenerator.SequenceId(), serializer.serialize(apiMetadata));
                    this.enhancedDataSender.Send(request.ToBuffer());
                }
            }

            return result.Id;
        }
    }
}
