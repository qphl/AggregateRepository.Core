// <copyright file="IAggregateRepository.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.Core
{
    using CorshamScience.AggregateRepository.Core.Exceptions;

    /// <summary>
    /// Represents a method of persisting instances of <see cref="IAggregate"/>, handling both storage and access.
    /// </summary>
    public interface IAggregateRepository
    {
        /// <summary>
        /// Persists any uncommitted events on the <see cref="IAggregate"/> to the underlying storage.
        /// </summary>
        /// <param name="aggregateToSave">The <see cref="IAggregate"/> to save.</param>
        /// <exception cref="AggregateVersionException">
        /// Thrown if <see cref="IAggregate.Version"/> does not match the current version in the underlying storage + the number of new events to commit.
        /// </exception>
        /// <exception cref="AggregateNotFoundException">
        /// See implementation specific documentation for the circumstances in which this may be thrown.
        /// </exception>
        void Save(IAggregate aggregateToSave);

        /// <summary>
        /// Rehydrates an aggregate of the specified type from events in the underlying storage.
        /// </summary>
        /// <typeparam name="T">The aggregate type to be instantiated.</typeparam>
        /// <param name="aggregateId">The ID of the aggregate to be loaded.</param>
        /// <param name="version">
        /// The version at which to load the aggregate. No events beyond this will be loaded.
        /// </param>
        /// <returns>
        /// Returns an aggregate instance which has had historic events passed to its
        /// <see cref="IAggregate.ApplyEvent"/> method.
        /// </returns>
        T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue)
            where T : IAggregate;
    }
}
