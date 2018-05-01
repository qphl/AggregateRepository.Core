// <copyright file="AggregateBase.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for building an Aggregate.
    /// </summary>
    public abstract class AggregateBase : IAggregate
    {
#pragma warning disable SA1401 // Fields should be private - Used in external systems.
        /// <summary>
        /// Used for doing nothing when applying an event.
        /// </summary>
        protected static Action<object> noop = e => { };
#pragma warning restore SA1401 // Fields should be private

        private readonly List<object> _changes = new List<object>();

        private EventMap _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateBase"/> class.
        /// </summary>
        public AggregateBase()
        {
            _map = Map;
        }

        /// <inheritdoc />
        public abstract object Id { get; }

        /// <inheritdoc />
        public int Version { get; private set; }

        /// <summary>
        /// Gets the EventMap used within the aggregate.
        /// </summary>
        protected abstract EventMap Map { get; }

        /// <summary>
        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">If an event is not mapped then an InvalidOperationException will be thrown </exception>
        /// </summary>
        /// <inheritdoc />
        public void ApplyEvent(object @event)
        {
            Action<object> handler;
            if (_map.TryGetValue(@event.GetType(), out handler))
            {
                handler(@event);
                Version++;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0} can not be applied to {1}. No appropriate mapping is registered",
                        @event.GetType().Name,
                        GetType().Name));
            }
        }

        /// <inheritdoc />
        public ICollection GetUncommittedEvents()
        {
            return _changes;
        }

        /// <inheritdoc />
        public void ClearUncommittedEvents()
        {
            _changes.Clear();
        }

        /// <summary>
        /// Raises the event that is passed in.
        /// </summary>
        /// <param name="event">Event Data</param>
        protected void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            _changes.Add(@event);
        }
    }
}
