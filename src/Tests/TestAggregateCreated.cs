namespace CR.AggregateRepository.Tests
{
    internal class TestAggregateCreated
    {
        public readonly object AggregateId;

        public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;
    }
}
