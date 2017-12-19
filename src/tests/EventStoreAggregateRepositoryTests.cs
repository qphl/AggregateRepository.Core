using CR.AggregateRepository.Persistance.EventStore;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;

namespace CR.AggregateRepository.Tests
{
    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private IEventStoreConnection _connection;
        private EmbeddedEventStore _eventStore;

        protected override void InitRepository()
        {
            _eventStore = new EmbeddedEventStore(11113, 12113);
            _eventStore.Start();

            _connection = EmbeddedEventStoreConnection.Create(_eventStore.Node);
            _connection.ConnectAsync().Wait();
            _repoUnderTest = new EventStoreAggregateRepository(_connection);
        }

        protected override void CleanUpRepository()
        {
            _connection.Close();
            _eventStore.Stop();
        }
    }
}