namespace Pinpoint.Agent.Common
{
    using System.Diagnostics;

    public static class AgentStat
    {
        private static PerformanceCounter systemCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public static double GetCpuLoad()
        {
            //return systemCpu.NextValue() / (double)100;
            return 0;
        }
    }
}
