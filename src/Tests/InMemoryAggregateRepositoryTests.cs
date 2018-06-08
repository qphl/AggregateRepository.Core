using CR.AggregateRepository.Persistence.Memory;

namespace CR.AggregateRepository.Tests
{
    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository() => RepoUnderTest = new InMemoryAggregateRepository();

        protected override void CleanUpRepository()
        {
        }
    }
}
