// <copyright file="IAggregate.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System.Collections;

    /// <summary>
    /// Represents a CQRS aggregate root which consumes events to build it's state, and creates events to save changes to it's state.
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Gets a unique identifier for this aggregate instance.
        /// The ToString() override of this object will be used to determine a storage location
        /// for the events belonging to this aggregate.
        /// </summary>
        object Id { get; }

        /// <summary>
        /// Gets the current Aggregate Version.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Applies an event to the aggregate.
        /// </summary>
        /// <remarks>
        /// This is the mechanism by which aggregates are rehydrated by an <see cref="IAggregateRepository"/>.
        /// </remarks>
        /// <param name="event">The Event object to apply to the aggregate.</param>
        void ApplyEvent(object @event);

        /// <summary>
        /// Gets an <see cref="ICollection"/> of events which have been produced since the aggregate was loaded.
        /// </summary>
        /// <returns>A collection of uncommited events.</returns>
        ICollection GetUncommittedEvents();

        /// <summary>
        /// Clears all uncommitted events attributed to the aggregate.
        /// </summary>
        void ClearUncommittedEvents();
    }
}
