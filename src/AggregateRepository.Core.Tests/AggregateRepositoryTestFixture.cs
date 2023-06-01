// <copyright file="AggregateRepositoryTestFixture.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CorshamScience.AggregateRepository.Core;
    using CorshamScience.AggregateRepository.Core.Exceptions;
    using NUnit.Framework;

    [TestFixture]
    public abstract class AggregateRepositoryTestFixture
    {
        private List<Guid> _storedEvents = new List<Guid>();
        private TestAggregate _retrievedAggregate;
        private string _aggregateIdUnderTest;

        protected IAggregateRepository RepoUnderTest { get; set; }

        [SetUp]
        public async Task SetUp()
        {
            await InitRepository();
            _aggregateIdUnderTest = Guid.NewGuid().ToString();
            _storedEvents = new List<Guid>();
        }

        [TearDown]
        public async Task TearDown() => await CleanUpRepository();

        [Test]
        public void Retreiving_an_aggregate_from_an_empty_eventstore_should_throw_an_exception() => Assert.ThrowsAsync<AggregateNotFoundException>(async () => await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest));

        [Test]
        public async Task Retreiving_a_nonexistant_aggregate_id_should_throw_an_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 2; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);
            Assert.ThrowsAsync<AggregateNotFoundException>(async () => await RepoUnderTest.GetAggregateAsync<TestAggregate>(Guid.NewGuid().ToString()));
        }

        [Test]
        public async Task Retrieving_a_newly_created_aggregate_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            await RepoUnderTest.SaveAsync(aggregate);

            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest);
            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(0, _retrievedAggregate.EventsApplied.Count);
        }

        [Test]
        public async Task Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);
            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public async Task Retrieving_an_aggregate_with_events_when_specifying_a_version_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);
            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest, 6);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public async Task Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly_when_the_event_store_contains_multiple_aggregates()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);
            var secondAggregate = new TestAggregate(Guid.NewGuid().ToString());
            for (var i = 0; i < 6; i++)
            {
                var eventId = Guid.NewGuid();
                secondAggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(secondAggregate);
            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);
            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.EventsApplied.Count);
            foreach (var id in _storedEvents)
            {
                Assert.Contains(id, _retrievedAggregate.EventsApplied);
            }
        }

        [Test]
        public async Task Saving_new_events_to_an_existing_aggregate_should_correctly_persist_events()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            await RepoUnderTest.SaveAsync(aggregate);

            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest);

            var eventId = Guid.NewGuid();
            _retrievedAggregate.GenerateEvent(eventId);

            await RepoUnderTest.SaveAsync(_retrievedAggregate);
            var actualAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(1, actualAggregate.EventsApplied.Count);
            Assert.AreEqual(_aggregateIdUnderTest, actualAggregate.Id);
            Assert.AreEqual(eventId, actualAggregate.EventsApplied[0]);
        }

        [Test]
        public async Task Saving_an_aggregate_with_expected_version_less_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);

            _retrievedAggregate = await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest, 3);

            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                _retrievedAggregate.GenerateEvent(eventId);
            }

            Assert.ThrowsAsync<AggregateVersionException>(async () => await RepoUnderTest.SaveAsync(_retrievedAggregate));
        }

        [Test]
        public async Task Retrieving_an_aggregate_with_expected_version_greater_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            await RepoUnderTest.SaveAsync(aggregate);
            Assert.ThrowsAsync<AggregateVersionException>(async () => await RepoUnderTest.GetAggregateAsync<TestAggregate>(_aggregateIdUnderTest, 10));
        }

        protected abstract Task InitRepository();

        protected abstract Task CleanUpRepository();
    }
}
