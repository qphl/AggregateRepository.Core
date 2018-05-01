// <copyright file="IAggregateRepository.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    /// <summary>
    /// Interface for building AggregateRepository classes.
    /// </summary>
    public interface IAggregateRepository
    {
        /// <summary>
        /// Saves the aggregate that is passed in.
        /// </summary>
        /// <param name="aggregateToSave">Aggregate which requires saving.</param>
        void Save(IAggregate aggregateToSave);

        /// <summary>
        /// Gets an Aggregate based on Type and Id.
        /// </summary>
        /// <typeparam name="T">Type of aggregate we want to get.</typeparam>
        /// <param name="aggregateId">Id of the aggregate we want to get.</param>
        /// <param name="version">Version of the aggregate.</param>
        /// <returns>Returns an aggregate.</returns>
        T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue)
            where T : IAggregate;
    }
}
