using CR.AggregateRepository.Core;

namespace CR.AggregateRepository.Persistance.Memory
{
    public class InMemoryAggregateRepository : IAggregateRepository
    {
        public void Save(IAggregate aggregateToSave, int expectedAggregateVersion)
        {
            throw new System.NotImplementedException();
        }

        public T GetAggregateFromRepository<T>(string aggregateId) where T : IAggregate
        {
            throw new System.NotImplementedException();
        }
    }
}
