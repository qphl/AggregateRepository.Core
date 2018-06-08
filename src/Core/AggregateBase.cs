// <copyright file="AggregateBase.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// The abstract base class for building an aggregate. This can be inherited from to allow for building, manipulating and updating an instance of an object through a collection of events.
    /// </summary>
    public abstract class AggregateBase : IAggregate
    {
#pragma warning disable SA1401 // Fields should be private - Used in external systems.
        /// <summary>
        /// Used for doing nothing when applying an event.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        protected static Action<object> noop = e => { };
#pragma warning restore SA1401 // Fields should be private

        private readonly List<object> _changes = new List<object>();
        private readonly EventMap _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateBase"/> class.
        /// </summary>
        // ReSharper disable once VirtualMemberCallInConstructor
        // ReSharper disable once UnusedMember.Global
        protected AggregateBase() => _map = Map;

        /// <inheritdoc />
        public int Version { get; private set; }

        /// <inheritdoc />
        public abstract object Id { get; }

        /// <summary>
        /// Gets the <see cref="EventMap"/> used internally by the aggregate.
        /// </summary>
        protected abstract EventMap Map { get; }

        /// <summary>
        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">If the provided event's <see cref="Type"/> is not mapped in the internal <see cref="EventMap"/>, an <see cref="InvalidOperationException"/> will be thrown.</exception>
        /// </summary>
        /// <inheritdoc />
        public void ApplyEvent(object @event)
        {
            if (_map.TryGetValue(@event.GetType(), out var handler))
            {
                handler(@event);
                Version++;
            }
            else
            {
                throw new InvalidOperationException($"{@event.GetType().Name} can not be applied to {GetType().Name}. No appropriate mapping is registered");
            }
        }

        /// <inheritdoc />
        public ICollection GetUncommittedEvents() => _changes;

        /// <inheritdoc />
        public void ClearUncommittedEvents() => _changes.Clear();

        /// <summary>
        /// Raises the event that is passed in against the aggredate, applying any necessary changes to keep the aggregate's state up to date.
        /// </summary>
        /// <param name="event">Event Data</param>
        protected void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            _changes.Add(@event);
        }
    }
}
