using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.AggregateRepository.Core;

namespace tests
{
    public abstract class AggregateRepositoryTestFixture
    {

        protected IAggregateRepository Repository { get; set; }


    }
}
