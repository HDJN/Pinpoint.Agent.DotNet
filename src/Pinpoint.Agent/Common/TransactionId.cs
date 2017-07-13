namespace Pinpoint.Agent.Common
{
    using System;
    using System.Text;

    public class TransactionId
    {
        public string AgentId { get; private set; }

        public long AgentStartTime { get; private set; }

        public long TransactionSequence { get; private set; }

        public TransactionId(String agentId, long agentStartTime, long transactionSequence)
        {
            if (agentId == null)
            {
                throw new NullReferenceException("agentId must not be null");
            }
            AgentId = agentId;
            AgentStartTime = agentStartTime;
            TransactionSequence = transactionSequence;
        }

        public TransactionId(long agentStartTime, long transactionSequence)
        {
            AgentStartTime = agentStartTime;
            TransactionSequence = transactionSequence;
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            TransactionId that = (TransactionId)o;

            if (AgentStartTime != that.AgentStartTime) return false;
            if (TransactionSequence != that.TransactionSequence) return false;
            if (!AgentId.Equals(that.AgentId)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = AgentId.GetHashCode();
            result = 31 * result + (int)(AgentStartTime ^ (AgentStartTime >> 32));
            result = 31 * result + (int)(TransactionSequence ^ (TransactionSequence >> 32));
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("TransactionId{");
            sb.Append("agentId='").Append(AgentId).Append('\'');
            sb.Append(", agentStartTime=").Append(AgentStartTime);
            sb.Append(", transactionSequence=").Append(TransactionSequence);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
