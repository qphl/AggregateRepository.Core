// <copyright file="AggregateRepositoryTestFixture.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using CR.AggregateRepository.Core;
    using CR.AggregateRepository.Core.Exceptions;
    using NUnit.Framework;

    [TestFixture]
    public abstract class AggregateRepositoryTestFixture
    {
        private List<Guid> _storedEvents = new List<Guid>();
        private TestAggregate _retrievedAggregate;
        private string _aggregateIdUnderTest;

        protected IAggregateRepository RepoUnderTest { get; set; }

        [SetUp]
        public void SetUp()
        {
            InitRepository();
            _aggregateIdUnderTest = Guid.NewGuid().ToString();
            _storedEvents = new List<Guid>();
        }

        [TearDown]
        public void TearDown() => CleanUpRepository();

        [Test]
        public void Retreiving_an_aggregate_from_an_empty_eventstore_should_throw_an_exception() => Assert.Throws<AggregateNotFoundException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest));

        [Test]
        public void Retreiving_a_nonexistant_aggregate_id_should_throw_an_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 2; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            Assert.Throws<AggregateNotFoundException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(Guid.NewGuid().ToString()));
        }

        [Test]
        public void Retrieving_a_newly_created_aggregate_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);
            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(0, _retrievedAggregate.EventsApplied.Count);
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_when_specifying_a_version_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 6);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly_when_the_event_store_contains_multiple_aggregates()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            var secondAggregate = new TestAggregate(Guid.NewGuid().ToString());
            for (var i = 0; i < 6; i++)
            {
                var eventId = Guid.NewGuid();
                secondAggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(secondAggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public void Saving_new_events_to_an_existing_aggregate_should_correctly_persist_events()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            var eventId = Guid.NewGuid();
            _retrievedAggregate.GenerateEvent(eventId);

            RepoUnderTest.Save(_retrievedAggregate);
            var actualAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(1, actualAggregate.EventsApplied.Count);
            Assert.AreEqual(_aggregateIdUnderTest, actualAggregate.Id);
            Assert.AreEqual(eventId, actualAggregate.EventsApplied[0]);
        }

        [Test]
        public void Saving_an_aggregate_with_expected_version_less_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 3);

            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                _retrievedAggregate.GenerateEvent(eventId);
            }

            Assert.Throws<AggregateVersionException>(() => RepoUnderTest.Save(_retrievedAggregate));
        }

        [Test]
        public void Retrieving_an_aggregate_with_expected_version_greater_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            Assert.Throws<AggregateVersionException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 10));
        }

        protected abstract void InitRepository();

        protected abstract void CleanUpRepository();
    }
}
