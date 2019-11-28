// <copyright file="TestAggregateCreated.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    internal class TestAggregateCreated
    {
        public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

        public object AggregateId { get; }
    }
}
