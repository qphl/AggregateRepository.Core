using System;
using System.Collections.Generic;
using System.IO;
using CR.AggregateRepository.Persistance.EventStore;
using EventStore.ClientAPI;

namespace CR.AggregateRepository.Tests
{
    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private IEventStoreConnection _connection;
        private EmbeddedEventStore _eventStore;

        protected override void InitRepository()
        {
            _eventStore = new EmbeddedEventStore(GetTemporaryDirectory(), 11113, 12113);
            _eventStore.Start();

            _connection = EventStoreConnection.Create(_eventStore.TcpEndPoint);
            _connection.ConnectAsync().Wait();
            _repoUnderTest = new EventStoreAggregateRepository(_connection);
        }

        protected override void CleanUpRepository()
        {
            _connection.Close();
            _eventStore.Stop();
        }

        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}