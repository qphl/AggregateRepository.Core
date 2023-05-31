// <copyright file="InMemoryAggregateRepositoryTests.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace AggregateRepository.Core.Tests
{
    using CorshamScience.AggregateRepository.Core;

    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override Task InitRepository()
        {
            RepoUnderTest = new InMemoryAggregateRepository();
            return Task.CompletedTask;
        }

        protected override Task CleanUpRepository()
        {
            return Task.CompletedTask;
        }
    }
}
