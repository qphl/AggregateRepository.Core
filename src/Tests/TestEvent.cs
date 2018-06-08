using System;

namespace CR.AggregateRepository.Tests
{
    internal class TestEvent
    {
        public TestEvent(Guid eventId) => EventId = eventId;

        public Guid EventId { get; }
    }
}
