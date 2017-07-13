namespace Pinpoint.Agent.Common
{
    using Xunit;

    public class TransactionIdUtilsTest
    {
        [Fact(DisplayName = "TransactionIdUtils.test_parse_transaction_id")]
        public void test_parse_transaction_id()
        {
            var tranId = TransactionIdUtils.parseTransactionId("pp201705101110^1498710829244^1");
        }
    }
}
