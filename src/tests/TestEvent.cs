using System;
using System.Collections;
using CR.AggregateRepository.Core;

namespace CR.AggregateRepository.Tests
{
    internal class TestEvent
    {
        public TestEvent(String aggregateId, Guid eventId)
        {
            AggregateId = aggregateId;
            EventId = eventId;
        }

        public String AggregateId { get; private set; }
        public Guid EventId { get; private set; }
    }

    internal class TestAggregateCreated
    {
        public readonly String AggregateId;

        public TestAggregateCreated(String aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}