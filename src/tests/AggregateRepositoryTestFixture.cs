using System;
using System.Collections.Generic;
using CR.AggregateRepository.Core;
using CR.AggregateRepository.Core.Exceptions;
using NUnit.Framework;

namespace CR.AggregateRepository.Tests
{
    [TestFixture]
    public abstract class AggregateRepositoryTestFixture
    {
        protected IAggregateRepository _repoUnderTest { get; set; }

        private String _aggregateIdUnderTest;
        private TestAggregate _retrievedAggregate;
        private List<Guid> _storedEvents = new List<Guid>();

        protected abstract void InitRepository();
        protected abstract void CleanUpRepository();
       
        [SetUp]
        public void SetUp()
        {
            InitRepository();
            _aggregateIdUnderTest = Guid.NewGuid().ToString();
            _storedEvents = new List<Guid>();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUpRepository();
        }

        [Test]
        public void Retreiving_an_aggregate_from_an_empty_eventstore_should_throw_an_exception()
        {
            Assert.Throws<AggregateNotFoundException>(() => _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest));
        }

        [Test]
        public void Retreiving_a_nonexistant_aggregate_id_should_throw_an_exception()
        {

            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 2; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            Assert.Throws<AggregateNotFoundException>(() => _repoUnderTest.GetAggregateFromRepository<TestAggregate>(Guid.NewGuid().ToString()));
        }

        [Test]
        public void Retrieving_a_newly_created_aggregate_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            _repoUnderTest.Save(aggregate);

            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);

            Assert.AreEqual(0, _retrievedAggregate.eventsApplied.Count);
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);

            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.eventsApplied.Count);
            foreach (Guid id in _storedEvents)
                Assert.Contains(id, _retrievedAggregate.eventsApplied);
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_when_specifying_a_version_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest,6);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);

            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.eventsApplied.Count);
            foreach (Guid id in _storedEvents)
                Assert.Contains(id, _retrievedAggregate.eventsApplied);
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly_when_the_event_store_contains_multiple_aggregates()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            var secondAggregate = new TestAggregate(Guid.NewGuid().ToString());
            for (int i = 0; i < 6; i++)
            {
                Guid eventId = Guid.NewGuid();
                secondAggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(secondAggregate);

            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.AreEqual(_aggregateIdUnderTest, _retrievedAggregate.Id);

            Assert.AreEqual(_storedEvents.Count, _retrievedAggregate.eventsApplied.Count);
            foreach (Guid id in _storedEvents)
                Assert.Contains(id, _retrievedAggregate.eventsApplied);

        }

        [Test]
        public void Saving_new_events_to_an_existing_aggregate_should_correctly_persist_events()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            _repoUnderTest.Save(aggregate);

            //retrieve it
            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            var eventId = Guid.NewGuid();
            _retrievedAggregate.GenerateEvent(eventId);
            _repoUnderTest.Save(_retrievedAggregate);

            var actualAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);
            Assert.AreEqual(1, actualAggregate.eventsApplied.Count);
            Assert.AreEqual(_aggregateIdUnderTest, actualAggregate.Id);
            Assert.AreEqual(eventId, actualAggregate.eventsApplied[0]);
        }

        [Test]
        public void Saving_an_aggregate_with_expected_version_less_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            //retrieve it
            _retrievedAggregate = _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest,3);

            //even more events

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                _retrievedAggregate.GenerateEvent(eventId);
            }

            Assert.Throws<AggregateVersionException>(() => _repoUnderTest.Save(_retrievedAggregate)); //this version will be less than actual

        }

        [Test]
        public void
            Retrieving_an_aggregate_with_expected_version_greater_than_the_actual_version_should_throw_a_concurrency_exception
            ()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);

            for (int i = 0; i < 5; i++)
            {
                Guid eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            _repoUnderTest.Save(aggregate);

            //retrieve it
            Assert.Throws<AggregateVersionException>(() => _repoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 10));
        }

    }
}
