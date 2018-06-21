// <copyright file="InMemoryAggregateRepositoryTests.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    using CR.AggregateRepository.Core;

    public class InMemoryAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository() => RepoUnderTest = new InMemoryAggregateRepository();

        protected override void CleanUpRepository()
        {
        }
    }
}
