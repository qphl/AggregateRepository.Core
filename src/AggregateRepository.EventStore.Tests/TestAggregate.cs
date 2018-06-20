// <copyright file="TestAggregate.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.EventStore.Tests
{
    using System;
    using System.Collections.Generic;
    using CR.AggregateRepository.Core;

    internal sealed class TestAggregate : AggregateBase
    {
        private object _id;

        public TestAggregate(string aggregateId) => RaiseEvent(new TestAggregateCreated(aggregateId));

        // ReSharper disable once UnusedMember.Local
        private TestAggregate()
        {
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public override object Id => _id;

        public List<Guid> EventsApplied { get; } = new List<Guid>();

        protected override EventMap Map => new EventMap
        {
            [typeof(TestEvent)] = e => Apply((TestEvent)e),
            [typeof(TestAggregateCreated)] = e => Apply((TestAggregateCreated)e),
        };

        public void Apply(TestEvent e) => EventsApplied.Add(e.EventId);

        public void Apply(TestAggregateCreated e) => _id = e.AggregateId;

        public void GenerateEvent(Guid eventId) => RaiseEvent(new TestEvent(eventId));
    }
}
