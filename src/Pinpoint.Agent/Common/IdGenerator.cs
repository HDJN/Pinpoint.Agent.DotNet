namespace Pinpoint.Agent.Common
{
    using System.Threading;

    public class IdGenerator
    {
        private static int id = 0;

        public static int SequenceId()
        {
            return Interlocked.Increment(ref id);
        }
    }
}
