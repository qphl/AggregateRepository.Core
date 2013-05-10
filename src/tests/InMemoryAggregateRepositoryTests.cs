using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.AggregateRepository.Core.Exceptions;
using CR.AggregateRepository.Persistance.Memory;
using NUnit.Framework;

namespace CR.AggregateRepository.Tests
{
    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository()
        {
            _repoUnderTest = new InMemoryAggregateRepository();
        }
    }
}
