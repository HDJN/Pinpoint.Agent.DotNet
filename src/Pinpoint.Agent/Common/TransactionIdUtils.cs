namespace Pinpoint.Agent.Common
{
    using Buffer;
    using System;
    using System.Text;

    public class TransactionIdUtils
    {
        // value is displayed as html - should not use html syntax
        public static readonly char TRANSACTION_ID_DELIMITER = '^';
        public static readonly byte VERSION = 0;

        private TransactionIdUtils()
        {
        }

        public static String formatString(TransactionId transactionId)
        {
            return formatString(transactionId.AgentId, transactionId.AgentStartTime, transactionId.TransactionSequence);
        }

        public static String formatString(String agentId, long agentStartTime, long transactionSequence)
        {
            if (agentId == null)
            {
                throw new NullReferenceException("agentId must not be null");
            }
            StringBuilder sb = new StringBuilder(64);
            sb.Append(agentId);
            sb.Append(TRANSACTION_ID_DELIMITER);
            sb.Append(agentStartTime);
            sb.Append(TRANSACTION_ID_DELIMITER);
            sb.Append(transactionSequence);
            return sb.ToString();
        }

        public static byte[] formatBytes(String agentId, long agentStartTime, long transactionSequence)
        {
            return writeTransactionId(agentId, agentStartTime, transactionSequence);
        }

        public static byte[] formatByteBuffer(String agentId, long agentStartTime, long transactionSequence)
        {
            return writeTransactionId(agentId, agentStartTime, transactionSequence);
        }

        private static byte[] writeTransactionId(String agentId, long agentStartTime, long transactionSequence)
        {
            // agentId may be null
            // version + prefixed size + string + long + long
            var buffer = new AutomaticBuffer(1 + 5 + 24 + 10 + 10);
            buffer.putByte(VERSION);
            buffer.putPrefixedString(agentId);
            buffer.putVLong(agentStartTime);
            buffer.putVLong(transactionSequence);
            return buffer.getBuffer();
        }

        public static TransactionId parseTransactionId(byte[] transactionId)
        {
            if (transactionId == null)
            {
                throw new NullReferenceException("transactionId must not be null");
            }
            var buffer = new FixedBuffer(transactionId);
            var version = buffer.readByte();
            if (version != VERSION)
            {
                throw new Exception("invalid Version");
            }

            var agentId = buffer.readPrefixedString();
            var agentStartTime = buffer.readVLong();
            var transactionSequence = buffer.readVLong();
            if (agentId == null)
            {
                return new TransactionId(agentStartTime, transactionSequence);
            }
            else
            {
                return new TransactionId(agentId, agentStartTime, transactionSequence);
            }
        }

        public static TransactionId parseTransactionId(String transactionId)
        {
            if (transactionId == null)
            {
                throw new NullReferenceException("transactionId must not be null");
            }

            var args = transactionId.Split(TRANSACTION_ID_DELIMITER);
            if (args.Length < 1)
            {
                throw new Exception("agentIndex not found:" + transactionId);
            }
            var agentId = args[0];

            if (args.Length < 2)
            {
                throw new Exception("agentStartTimeIndex not found:" + transactionId);
            }
            var agentStartTime = parseLong(args[1]);

            if (args.Length < 3)
            {
                throw new Exception("transactionSequence not found:" + transactionId);
            }
            var transactionSequence = parseLong(args[2]);
            return new TransactionId(agentId, agentStartTime, transactionSequence);
        }

        private static int nextIndex(String transactionId, int fromIndex)
        {
            return transactionId.IndexOf(TRANSACTION_ID_DELIMITER, fromIndex);
        }

        private static long parseLong(String longString)
        {
            try
            {
                return Int64.Parse(longString);
            }
            catch (FormatException e)
            {
                throw new FormatException("parseLong Error. " + longString);
            }
        }
    }
}
