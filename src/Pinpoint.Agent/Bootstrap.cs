namespace Pinpoint.Profiler
{
    using Agent;
    using Agent.Common;
    using Agent.Context;
    using Agent.DotNet.Configuration;
    using Agent.Meta;
    using Agent.Network;
    using Agent.Thrift.Dto;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data.Common;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using TinyIoC;

    public static class Bootstrap
    {
        private static Logger logger = Logger.Current;

        private static PinpointUdpClient client = new PinpointUdpClient();

        private const string contextItemNamePrefix = "Pinpoint";
        private const string tSpanItemName = contextItemNamePrefix + ".TSPan";
        private const string preIdItemName = contextItemNamePrefix + ".PreId";
        private const string callStackItemName = contextItemNamePrefix + ".CallStack";

        public static void InterceptMethodBegin(string className, string methodName)
        {
            InterceptMethodBegin(className, methodName, new object[] { });
        }

        public static void InterceptMethodBegin(string className, string methodName, object arg)
        {
            InterceptMethodBegin(className, methodName, new object[] { arg });
        }

        public static void InterceptMethodBegin(string className, string methodName, object arg1, object arg2)
        {
            InterceptMethodBegin(className, methodName, new object[] { arg1, arg2 });
        }

        public static void InterceptMethodBegin(string className, string methodName, object[] args)
        {
            try
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    StartupIfNedd();

                    if (className == "CallHandlerExecutionStep" && methodName == "Execute")
                    {
                        // monitor entry
                        InitPerRequest(context);
                    }
                    else
                    {
                        if (context.Items[callStackItemName] == null)
                        {
                            return;
                        }
                    }

                    var methodId = DefaultAgentClient.CacheApi(new DefaultMethodDescriptor(className, methodName, null, null));

                    var span = (TSpan)context.Items[tSpanItemName];
                    var callStack = (Stack<TSpanEventWrapper>)context.Items[callStackItemName];
                    var preId = (short)context.Items[preIdItemName];
                    var spanEvent = new TSpanEvent()
                    {
                        Sequence = ++preId,
                        ServiceType = 1011,
                        Depth = callStack.Count + 1,
                        ApiId = methodId
                    };
                    spanEvent.__isset.nextSpanId = false;

                    if (callStack.Count == 0)
                    {
                        spanEvent.StartElapsed = 0;
                    }
                    else
                    {
                        spanEvent.StartElapsed = (int)(TimeUtils.GetCurrentTimestamp() - span.StartTime);
                    }

                    if (className == "CallHandlerExecutionStep" && methodName == "Execute")
                    {
                        var anns = new List<TAnnotation>();
                        var annsValue = new TAnnotationValue();
                        annsValue.StringValue = context.Request.Url.Query.TrimStart('?');
                        anns.Add(new TAnnotation() { Key = 41, Value = annsValue });
                        spanEvent.Annotations = anns;
                    }
                    else if (className == "System.Data.SqlClient.SqlCommand" &&
                        (methodName == "ExecuteNonQuery" || methodName == "ExecuteReader" || methodName == "ExecuteScalar"))
                    {
                        var sqlCmd = args[0] as DbCommand;
                        if (sqlCmd != null && sqlCmd.Connection != null)
                        {
                            spanEvent.DestinationId = sqlCmd.Connection.Database;
                            spanEvent.EndPoint = sqlCmd.Connection.DataSource;
                        }
                        spanEvent.ServiceType = 2101;
                        var anns = new List<TAnnotation>();
                        var annsValue = new TAnnotationValue();
                        var sqlId = DefaultAgentClient.CacheSql(DbParameterUtils.PretreatmentSql(sqlCmd.CommandText));
                        annsValue.IntStringStringValue = new TIntStringStringValue() { IntValue = sqlId };
                        if (sqlCmd.Parameters != null && sqlCmd.Parameters.Count > 0)
                        {
                            annsValue.IntStringStringValue.StringValue2 = DbParameterUtils.CollectionToString(sqlCmd.Parameters);
                        }
                        anns.Add(new TAnnotation() { Key = 20, Value = annsValue });
                        spanEvent.Annotations = anns;
                    }
                    else if (className == "MySql.Data.MySqlClient.MySqlCommand" && methodName == "ExecuteReader")
                    {
                        var sqlCmd = args[0] as DbCommand;
                        if (sqlCmd != null && sqlCmd.Connection != null)
                        {
                            spanEvent.DestinationId = sqlCmd.Connection.Database;
                            spanEvent.EndPoint = sqlCmd.Connection.DataSource;
                        }
                        spanEvent.ServiceType = 2101;
                        var anns = new List<TAnnotation>();
                        var annsValue = new TAnnotationValue();
                        var sqlId = DefaultAgentClient.CacheSql(DbParameterUtils.PretreatmentSql(sqlCmd.CommandText));
                        annsValue.IntStringStringValue = new TIntStringStringValue() { IntValue = sqlId };
                        if (sqlCmd.Parameters != null && sqlCmd.Parameters.Count > 0)
                        {
                            annsValue.IntStringStringValue.StringValue2 = DbParameterUtils.CollectionToString(sqlCmd.Parameters);
                        }
                        anns.Add(new TAnnotation() { Key = 20, Value = annsValue });
                        spanEvent.Annotations = anns;
                    }
                    else if (className == "System.Net.HttpWebRequest" && methodName == "GetResponse")
                    {
                        var uri = args[0] as Uri;
                        if (uri != null)
                        {
                            spanEvent.DestinationId = uri.Host;
                        }

                        var webHeaders = args[1] as WebHeaderCollection;
                        if (webHeaders != null)
                        {
                            SetHeaderValue(webHeaders, "pinpoint-traceid",
                                TransactionIdUtils.formatString(TransactionIdUtils.parseTransactionId(span.TransactionId)));
                            SetHeaderValue(webHeaders, "pinpoint-spanid", new Random().Next().ToString());
                            SetHeaderValue(webHeaders, "pinpoint-pspanid", span.SpanId.ToString());
                            SetHeaderValue(webHeaders, "pinpoint-flags", span.Flag.ToString());
                            SetHeaderValue(webHeaders, "pinpoint-pappname", span.ApplicationName);
                            SetHeaderValue(webHeaders, "pinpoint-papptype", span.ApplicationServiceType.ToString());
                            SetHeaderValue(webHeaders, "pinpoint-host", uri.Host);
                        }

                        spanEvent.ServiceType = 9055;
                        var anns = new List<TAnnotation>();
                        var annsValue = new TAnnotationValue();
                        annsValue.StringValue = uri.ToString();
                        anns.Add(new TAnnotation() { Key = 40, Value = annsValue });
                        spanEvent.Annotations = anns;
                    }
                    else if (className == "ServiceStack.Redis.RedisNativeClient" && methodName == "SendReceive")
                    {
                        var cmdWithBinaryArgs = args[0] as byte[][];
                        if (cmdWithBinaryArgs != null)
                        {
                            var anns = new List<TAnnotation>();
                            var annsValue = new TAnnotationValue();
                            var strBuilder = new StringBuilder();
                            Array.ForEach(cmdWithBinaryArgs, sub =>
                            {
                                strBuilder.AppendFormat("{0} ", Encoding.UTF8.GetString(sub));
                            });
                            annsValue.StringValue = strBuilder.ToString().TrimEnd();
                            anns.Add(new TAnnotation() { Key = -1, Value = annsValue });
                            spanEvent.Annotations = anns;
                        }
                        spanEvent.ServiceType = 8200;
                        spanEvent.DestinationId = "REDIS";
                        spanEvent.EndPoint = "Unknown";
                    }
                    else if (className == "RabbitMQ.Client.Impl.ModelBase" && methodName == "BasicPublish")
                    {
                        spanEvent.ServiceType = 8310;
                        spanEvent.DestinationId = "RabbitMQ";
                        spanEvent.EndPoint = "Unknown";
                    }

                    callStack.Push(new TSpanEventWrapper()
                    {
                        SpanEvent = spanEvent,
                        ClassName = className,
                        MethodName = methodName
                    });
                    context.Items[preIdItemName] = preId;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public static void InterceptMethodEnd(string className, string methodName)
        {
            try
            {
                var context = HttpContext.Current;
                if (context != null && context.Items[callStackItemName] != null)
                {
                    var span = (TSpan)context.Items[tSpanItemName];
                    var callStack = (Stack<TSpanEventWrapper>)context.Items[callStackItemName];

                    TSpanEventWrapper spanEventWrapper = null;
                    var size = callStack.Count;
                    for (var i = 0; i < size; i++)
                    {
                        spanEventWrapper = callStack.Pop();
                        if (spanEventWrapper.ClassName == className && spanEventWrapper.MethodName == methodName)
                        {
                            break;
                        }
                        else
                        {
                            if (i > 0)
                            {
                                logger.WarnFormat("end method stack error,className:{0},methodName:{1}", className, methodName);
                            }
                            if (i == size - 1)
                            {
                                return;
                            }
                        }
                    }

                    spanEventWrapper.SpanEvent.EndElapsed = (int)(TimeUtils.GetCurrentTimestamp() - span.StartTime - spanEventWrapper.SpanEvent.StartElapsed);
                    span.SpanEventList.Add(spanEventWrapper.SpanEvent);

                    if (callStack.Count == 0)
                    {
                        span.Elapsed = spanEventWrapper.SpanEvent.EndElapsed;

                        #region 划分到TSpanChunk

                        #endregion

                        client.Send(span);

                        CleanRequest(context);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private static void StartupIfNedd()
        {
            var agent = DefaultAgentClient.GetInstance();
            if (!agent.IsStart)
            {
                agent.Start();
            }
        }

        private static void InitPerRequest(HttpContext context)
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            var methodId = DefaultAgentClient.CacheApi(new DefaultMethodDescriptor(null, "IIS Process", null, null));

            var startTime = TimeUtils.GetCurrentTimestamp();

            var span = new TSpan()
            {
                AgentId = agentConfig.AgentId,
                ApplicationName = agentConfig.ApplicationName,
                AgentStartTime = agentConfig.AgentStartTime,
                TransactionId = TransactionIdUtils.formatBytes(agentConfig.AgentId, agentConfig.AgentStartTime, IdGenerator.SequenceId()),
                SpanId = new Random().Next(),
                StartTime = startTime,
                Rpc = context.Request.Path,
                ServiceType = 1010,
                EndPoint = Dns.GetHostAddresses(Dns.GetHostName())[0].ToString(),
                RemoteAddr = context.Request.UserHostAddress,
                Annotations = null,
                Flag = 0,
                ApiId = methodId,
                ApplicationServiceType = 1010,
                SpanEventList = new List<TSpanEvent>()
            };
            span.__isset.parentSpanId = false;
            span.__isset.err = false;

            var traceid = context.Request.Headers["pinpoint-traceid"];
            var spanid = context.Request.Headers["pinpoint-spanid"];
            var pspanid = context.Request.Headers["pinpoint-pspanid"];
            var flags = context.Request.Headers["pinpoint-flags"];
            var pappname = context.Request.Headers["pinpoint-pappname"];
            var papptype = context.Request.Headers["pinpoint-papptype"];
            var host = context.Request.Headers["pinpoint-host"];
            if (!String.IsNullOrEmpty(traceid))
            {
                var tid = TransactionIdUtils.parseTransactionId(traceid);
                span.TransactionId = TransactionIdUtils.formatBytes(tid.AgentId, tid.AgentStartTime, tid.TransactionSequence);
                span.SpanId = long.Parse(spanid);
                span.ParentSpanId = long.Parse(pspanid);
                span.Flag = short.Parse(flags);
                span.ParentApplicationName = pappname;
                span.ParentApplicationType = short.Parse(papptype);
                span.AcceptorHost = host;
            }

            context.Items.Add(tSpanItemName, span);

            context.Items.Add(preIdItemName, Int16.Parse("-1"));

            context.Items.Add(callStackItemName, new Stack<TSpanEventWrapper>());
        }

        private static void CleanRequest(HttpContext context)
        {
            context.Items[tSpanItemName] = null;
            context.Items[callStackItemName] = null;
        }

        private static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }
    }
}
