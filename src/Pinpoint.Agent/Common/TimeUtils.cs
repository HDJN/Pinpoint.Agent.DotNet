namespace Pinpoint.Agent.Common
{
    using System;

    public class TimeUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>unit of time is a millisecond</returns>
        public static long GetCurrentTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
    }
}
