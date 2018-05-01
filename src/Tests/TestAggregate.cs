﻿using System;
using System.Collections.Generic;
using CR.AggregateRepository.Core;



namespace CR.AggregateRepository.Tests
{
    internal class TestAggregate : AggregateBase
    {
        private object _id;

        public override object Id
        {
            get { return _id; }
        }

        public List<Guid> eventsApplied = new List<Guid>();

        private readonly List<Object> _changes = new List<Object>();

        public TestAggregate(string aggregateId)
        {
            RaiseEvent(new TestAggregateCreated(aggregateId));
        }

        private TestAggregate() { }

        protected override EventMap Map => new EventMap
        {
            [typeof(TestEvent)] = e => Apply((TestEvent) e),
            [typeof(TestAggregateCreated)] = e => Apply((TestAggregateCreated) e)
        };

        public void Apply(TestEvent e)
        {
            eventsApplied.Add(e.EventId);
        }

        public void Apply(TestAggregateCreated e)
        {
            _id = e.AggregateId;
        }

        public void GenerateEvent(Guid eventId)
        {
            RaiseEvent(new TestEvent(eventId));
        }

    }
}