extern alias EventStoreNetFrameworkClient;
extern alias EventStoreNetCoreClient;
using CR.AggregateRepository.Persistence.EventStore;
using EventStore.ClientAPI.Embedded;

namespace CR.AggregateRepository.Tests
{
    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private EventStoreNetFrameworkClient.EventStore.ClientAPI.IEventStoreConnection _connection;
        private EmbeddedEventStore _eventStore;

        protected override void InitRepository()
        {
            _eventStore = new EmbeddedEventStore(11113, 12113);
            _eventStore.Start();

            _connection = EmbeddedEventStoreConnection.Create(_eventStore.Node);
            _connection.ConnectAsync().Wait();

            RepoUnderTest = new EventStoreAggregateRepository(new ConnectionTranslator(_connection));
        }

        protected override void CleanUpRepository()
        {
            _connection.Close();
            _eventStore.Stop();
        }
    }
}
