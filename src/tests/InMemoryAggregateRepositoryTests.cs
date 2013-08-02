using CR.AggregateRepository.Persistance.Memory;

namespace CR.AggregateRepository.Tests
{
    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository()
        {
            _repoUnderTest = new InMemoryAggregateRepository();
        }

        protected override void CleanUpRepository()
        {
            
        }
    }
}
