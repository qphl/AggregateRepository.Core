namespace CR.AggregateRepository.Core
{
    public interface IAggregateRepository
    {
        void Save(IAggregate aggregateToSave);
        T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue) where T : IAggregate;
    }
}
