// <copyright file="TestAggregateCreated.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.Core.Tests
{
    internal class TestAggregateCreated
    {
        public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

        public object AggregateId { get; }
    }
}
