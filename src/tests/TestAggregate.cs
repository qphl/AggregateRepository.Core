using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.AggregateRepository.Core;

namespace CR.AggregateRepository.Tests
{
    internal class TestAggregate : IAggregate
    {
        public String Id { get; private set; }
        public int Version { get; private set; }

        public List<Guid> eventsApplied = new List<Guid>();

        private readonly List<Object> _changes = new List<Object>();

        public TestAggregate(string aggregateId)
        {
            RaiseEvent(new TestAggregateCreated(aggregateId));
        }

        private TestAggregate()
        {
            
        }

        public void Apply(TestEvent e)
        {
            eventsApplied.Add(e.EventId);
        }

        public void Apply(TestAggregateCreated e)
        {
            Id = e.AggregateId;
        }

        public void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            _changes.Add(@event);
        }

        public void ApplyEvent(object @event)
        {
            ((dynamic) this).Apply((dynamic) @event);
            Version++;
        }

        public ICollection GetUncommittedEvents()
        {
            return _changes;
        }

        public void ClearUncommittedEvents()
        {
            _changes.Clear();
        }

        public void GenerateEvent(Guid eventId)
        {
            RaiseEvent(new TestEvent(Id, eventId));
        }

    }
}
