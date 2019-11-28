// <copyright file="InMemoryAggregateRepositoryTests.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    using CorshamScience.AggregateRepository.Core;

    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository() => RepoUnderTest = new InMemoryAggregateRepository();

        protected override void CleanUpRepository()
        {
        }
    }
}
