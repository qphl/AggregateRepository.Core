using System;
using System.Collections;
using CR.AggregateRepository.Core;

namespace CR.AggregateRepository.Tests
{
    internal class TestEvent
    {
        public TestEvent(Guid eventId)
        {
            EventId = eventId;
        }

        public Guid EventId { get; private set; }
    }

    internal class TestAggregateCreated
    {
        public readonly Object AggregateId;

        public TestAggregateCreated(Object aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}