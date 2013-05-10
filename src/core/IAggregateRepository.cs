using System;

namespace CR.AggregateRepository.Core
{
    public interface IAggregateRepository
    {
        void Save(IAggregate aggregateToSave, int expectedAggregateVersion);
        T GetAggregateFromRepository<T>(String aggregateId) where T : IAggregate;
    }
}
