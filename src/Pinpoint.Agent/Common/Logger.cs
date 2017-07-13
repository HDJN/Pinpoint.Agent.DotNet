namespace Pinpoint.Agent.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class Logger
    {
        private static Logger logger = null;

        private string logFile = string.Empty;

        private ConcurrentQueue<String> msgQueue = null;

        private Timer flushMsgTimer = null;

        private AutoResetEvent flushMsgThreadSignal = null;

        public static Logger Current
        {
            get { return logger; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName">to solve delopy multiple website in same machine,write file conflict</param>
        private Logger(string applicationName)
        {
            var homeDirectory = Environment.GetEnvironmentVariable("PINPOINT_HOME");
            logFile = String.Format("{0}\\logs\\{1}.log", homeDirectory.TrimEnd('\\'), applicationName);

            msgQueue = new ConcurrentQueue<string>();

            flushMsgTimer = new Timer(FlushMsg, null, 1000, 1000);

            flushMsgThreadSignal = new AutoResetEvent(true);
        }

        /// <summary>
        /// non thread safety
        /// </summary>
        /// <param name="applicationName"></param>
        public static void Init(string applicationName)
        {
            logger = new Logger(applicationName);
        }

        public void Info(string msg)
        {
            WriteMsg(Level.INFO, msg);
        }

        public void InfoFormat(string msg, params object[] args)
        {
            WriteMsg(Level.INFO, String.Format(msg, args));
        }

        public void Warn(string msg)
        {
            WriteMsg(Level.WARN, msg);
        }

        public void WarnFormat(string msg, params object[] args)
        {
            WriteMsg(Level.WARN, String.Format(msg, args));
        }

        public void Error(string msg)
        {
            WriteMsg(Level.ERROR, msg);
        }

        public void ErrorFormat(string msg, params object[] args)
        {
            WriteMsg(Level.ERROR, String.Format(msg, args));
        }

        private void WriteMsg(Level level, string msg)
        {
            msgQueue.Enqueue(String.Format("{0} {1} {2}\r\n",
                DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"), level, msg));
        }

        private void FlushMsg(object state)
        {
            if (!flushMsgThreadSignal.WaitOne(1))
            {
                return;
            }

            try
            {
                string msg;
                using (var file = new FileStream(logFile, FileMode.Append))
                {
                    while (msgQueue.TryDequeue(out msg))
                    {
                        var bytes = Encoding.UTF8.GetBytes(msg);
                        file.Write(bytes, 0, bytes.Length);
                    }
                    file.Flush();
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.ToString());
            }
            finally
            {
                flushMsgThreadSignal.Set();
            }
        }

        private enum Level
        {
            INFO = 1,
            WARN = 2,
            ERROR = 3
        }
    }
}
