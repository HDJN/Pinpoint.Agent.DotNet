namespace Pinpoint.Agent
{
    using Configuration;
    using DotNet.Configuration;
    using Meta;
    using Thrift.Dto;
    using Thrift.IO;
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using TinyIoC;
    using Network;
    using Packet;
    using Pinpoint;

    public class DefaultAgentClient
    {
        private const string agentVersion = "1.7.0-SNAPSHOT";

        private DefaultPinpointTcpClient tcpClient = new DefaultPinpointTcpClient();

        private static DefaultApiMetaDataService dataService = null;

        private static DefaultSqlMetaDataService sqlDataService = null;

        private Thread sendAgentInfoThread;

        private Thread sendAgentStatInfoThread;

        private static Object locker = new Object();

        private static DefaultAgentClient instance = null;

        private static bool isStart = false;

        public bool IsStart
        {
            get
            {
                return isStart;
            }
        }

        private DefaultAgentClient()
        {

        }

        public static DefaultAgentClient GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new DefaultAgentClient();
                    }
                }
            }

            return instance;
        }

        public void Start()
        {
            lock (locker)
            {
                if (isStart)
                {
                    return;
                }

                InitAgentContext();

                var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();

                Logger.Init(agentConfig.ApplicationName);

                dataService = new DefaultApiMetaDataService(agentConfig.AgentId, agentConfig.AgentStartTime, tcpClient);
                sqlDataService = new DefaultSqlMetaDataService(agentConfig.AgentId, agentConfig.AgentStartTime, tcpClient);

                new Thread(StartAgent).Start();
                isStart = true;

                Logger.Current.Info("Pinpoint Agent Started");
            }
        }

        public static int CacheApi(MethodDescriptor methodDescriptor)
        {
            methodDescriptor.ApiId = methodDescriptor.ApiId = IdGenerator.SequenceId();
            return dataService.CacheApi(methodDescriptor);
        }
        public static int CacheSql(string sql)
        {
            var parseResult = sqlDataService.ParseSql(sql);
            parseResult.Id = IdGenerator.SequenceId();
            return sqlDataService.CacheSql(parseResult);
        }

        private void StartAgent()
        {
            try
            {
                HandShake();

                sendAgentInfoThread = new Thread(SendAgentInfo);
                sendAgentInfoThread.Start();

                sendAgentStatInfoThread = new Thread(SendAgentStatInfo);
                sendAgentStatInfoThread.Start();

            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.ToString());
            }
        }

        private void SendAgentInfo()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            while (true)
            {
                var agentInfo = new TAgentInfo
                {
                    AgentId = agentConfig.AgentId,
                    Hostname = agentConfig.HostName,
                    ApplicationName = agentConfig.ApplicationName,
                    AgentVersion = agentConfig.AgentVersion,
                    VmVersion = "1.8.0_121",
                    ServiceType = 1010,
                    StartTimestamp = agentConfig.AgentStartTime,
                    JvmInfo = new TJvmInfo()
                    {
                        Version = 0,
                        VmVersion = "1.8.0_121",
                        GcType = TJvmGcType.PARALLEL
                    }
                };

                try
                {
                    using (var serializer = new HeaderTBaseSerializer())
                    {
                        var payload = serializer.serialize(agentInfo);
                        var request = new RequestPacket(IdGenerator.SequenceId(), payload);
                        tcpClient.Send(request.ToBuffer());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error(ex.ToString());
                }

                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private void SendAgentStatInfo()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            while (true)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("10.10.11.70"), 9995);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                #region assemble agent stat batch entity

                var agentStatBatch = new TAgentStatBatch();
                agentStatBatch.AgentId = agentConfig.AgentId;
                agentStatBatch.StartTimestamp = agentConfig.AgentStartTime;
                agentStatBatch.AgentStats = new List<TAgentStat>();

                #endregion

                #region assemble agent stat entity

                var agentStat = new TAgentStat();
                agentStat.AgentId = agentConfig.AgentId;
                agentStat.StartTimestamp = agentConfig.AgentStartTime;
                agentStat.Timestamp = TimeUtils.GetCurrentTimestamp();
                agentStat.CollectInterval = 5000;
                agentStat.Gc = new TJvmGc()
                {
                    Type = TJvmGcType.PARALLEL,
                    JvmMemoryHeapUsed = 73842768,
                    JvmMemoryHeapMax = 436207616,
                    JvmMemoryNonHeapUsed = 196555576,
                    JvmMemoryNonHeapMax = -1,
                    JvmGcOldCount = 5,
                    JvmGcOldTime = 945,
                    JvmGcDetailed = new TJvmGcDetailed()
                    {
                        JvmGcNewCount = 110,
                        JvmGcNewTime = 1666,
                        JvmPoolCodeCacheUsed = 0.22167689005533855,
                        JvmPoolNewGenUsed = 0.025880894190828566,
                        JvmPoolOldGenUsed = 0.20353155869704026,
                        JvmPoolSurvivorSpaceUsed = 0.4635740007672991,
                        JvmPoolMetaspaceUsed = 0.9706939329583961
                    }
                };
                agentStat.CpuLoad = new TCpuLoad()
                {
                    JvmCpuLoad = 0.002008032128514056,
                    SystemCpuLoad = AgentStat.GetCpuLoad()
                };
                agentStat.Transaction = new TTransaction()
                {
                    SampledNewCount = 0,
                    SampledContinuationCount = 0,
                    UnsampledContinuationCount = 0,
                    UnsampledNewCount = 0
                };
                agentStat.ActiveTrace = new TActiveTrace()
                {
                    Histogram = new TActiveTraceHistogram()
                    {
                        Version = 0,
                        HistogramSchemaType = 2,
                        ActiveTraceCount = new List<int>() { 0, 0, 0, 0 }
                    }
                };
                agentStat.DataSourceList = new TDataSourceList()
                {
                    DataSourceList = new List<TDataSource>()
                {
                    new TDataSource()
                    {
                        Id = 1,
                        DatabaseName = "management",
                        ServiceTypeCode = 6050,
                        Url = "jdbc:mysql://10.10.12.50:3306/management?maxpoolsize=300",
                        MaxConnectionSize = 8
                    }
                }
                };

                #endregion

                for (var i = 0; i < 6; i++)
                {
                    agentStat.Timestamp -= 5000;
                    agentStatBatch.AgentStats.Add(agentStat);
                }

                try
                {
                    using (var serializer = new HeaderTBaseSerializer())
                    {
                        var data = serializer.serialize(agentStatBatch);
                        server.SendTo(data, ip);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error(ex.ToString());
                }

                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private void HandShake()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            var handshakeData = new Dictionary<string, object>();
            handshakeData.Add("serviceType", 1010);
            handshakeData.Add("socketId", 1);
            handshakeData.Add("hostName", agentConfig.HostName);
            handshakeData.Add("agentId", agentConfig.AgentId);
            handshakeData.Add("supportCommandList", new List<int> { 730, 740, 750, 710 });
            handshakeData.Add("ip", "192.168.56.1");
            handshakeData.Add("pid", 6496);
            handshakeData.Add("supportServer", true);
            handshakeData.Add("version", agentConfig.AgentVersion);
            handshakeData.Add("applicationName", agentConfig.ApplicationName);
            handshakeData.Add("startTimestamp", agentConfig.AgentStartTime);
            var payload = new ControlMessageEncoder().EncodeMap(handshakeData);
            var helloPacket = new ControlHandshakePacket(IdGenerator.SequenceId(), payload);
            tcpClient.Send(helloPacket.ToBuffer());
        }

        private void InitAgentContext()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            LoadAgentConfig(container);

            LoadPinpointConfig(container);
        }

        private void LoadAgentConfig(TinyIoCContainer container)
        {
            var agentConfig = new AgentConfig()
            {
                HostName = Dns.GetHostName(),
                AgentId = Environment.GetEnvironmentVariable("PINPOINT_AGENT_ID"),
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName(),
                AgentStartTime = TimeUtils.GetCurrentTimestamp(),
                AgentVersion = agentVersion
            };
            container.Register<AgentConfig>(agentConfig);
        }

        private void LoadPinpointConfig(TinyIoCContainer container)
        {
            var pinpointHome = Environment.GetEnvironmentVariable("PINPOINT_HOME");
            var configs = ConfigManager.Load(pinpointHome.TrimEnd('\\') + "\\pinpoint.config");
            var pinpointConfig = new PinpointConfig();
            var val = String.Empty;
            configs.TryGetValue("profiler.collector.ip", out val);
            pinpointConfig.CollectorIp = val;
            configs.TryGetValue("profiler.collector.span.port", out val);
            pinpointConfig.UpdSpanListenPort = int.Parse(val);
            configs.TryGetValue("profiler.collector.stat.port", out val);
            pinpointConfig.UdpStatListenPort = int.Parse(val);
            configs.TryGetValue("profiler.collector.tcp.port", out val);
            pinpointConfig.TcpListenPort = int.Parse(val);
            container.Register<PinpointConfig>(pinpointConfig);
        }
    }
}
