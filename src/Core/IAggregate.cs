// <copyright file="IAggregate.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System;
    using System.Collections;

    /// <summary>
    /// Interface for the Aggregate classes.
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Gets the Aggregate Id
        /// </summary>
        object Id { get; }

        /// <summary>
        /// Gets the Aggregate Versiob
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Apply an Event to the aggregate.
        /// </summary>
        /// <param name="event">Event object to process</param>
        void ApplyEvent(object @event);

        /// <summary>
        /// Gets any uncommitted events.
        /// </summary>
        /// <returns>A collection of uncommited events</returns>
        ICollection GetUncommittedEvents();

        /// <summary>
        /// Clears any uncommitted events.
        /// </summary>
        void ClearUncommittedEvents();
    }
}
