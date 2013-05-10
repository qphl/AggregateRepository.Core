using System;

namespace CR.AggregateRepository.Core
{
    public interface IAggregateRepository
    {
        void Save(IAggregate aggregateToSave);
        T GetAggregateFromRepository<T>(String aggregateId, int version) where T : IAggregate;
        T GetAggregateFromRepository<T>(String aggregateId) where T : IAggregate;
    }
}
