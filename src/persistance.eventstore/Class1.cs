using System;
using CR.AggregateRepository.Core;
using EventStore.ClientAPI;

namespace CR.AggregateRepository.Persistance.EventStore
{
    public class EventStoreAggregateRepository : IAggregateRepository
    {
        public EventStoreAggregateRepository(IEventStoreConnection connection)
        {
            
        }

        public void Save(IAggregate aggregateToSave)
        {
            throw new NotImplementedException();
        }

        public T GetAggregateFromRepository<T>(object aggregateId, int version) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public T GetAggregateFromRepository<T>(object aggregateId) where T : IAggregate
        {
            throw new NotImplementedException();
        }
    }
}
