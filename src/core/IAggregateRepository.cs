using System;

namespace CR.AggregateRepository.Core
{
    public interface IAggregateRepository
    {
        void Save(IAggregate aggregateToSave);
        T GetAggregateFromRepository<T>(object aggregateId, int version) where T : IAggregate;
        T GetAggregateFromRepository<T>(object aggregateId) where T : IAggregate;
    }
}
