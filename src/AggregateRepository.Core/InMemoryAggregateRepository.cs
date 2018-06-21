// <copyright file="InMemoryAggregateRepository.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    /// <inheritdoc />
    /// <summary>
    /// A simple in-memory aggregate repository implementation which reads events from, and writes them to, a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    public class InMemoryAggregateRepository : IAggregateRepository
    {
        /// <summary>
        /// In-memory event storage.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        public readonly ConcurrentDictionary<object, List<object>> EventStore = new ConcurrentDictionary<object, List<object>>();
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryAggregateRepository"/> class with an empty event store.
        /// </summary>
        public InMemoryAggregateRepository()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryAggregateRepository"/> class, with the provided streams (and events) contained in the provided <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="initialEvents">A dictionary of initial events added when itializing the instance.</param>
        // ReSharper disable once UnusedMember.Global
        public InMemoryAggregateRepository(IDictionary<object, List<object>> initialEvents)
        {
            foreach (var item in initialEvents)
            {
                EventStore.TryAdd(item.Key, new List<object>(item.Value));
            }
        }

        /// <inheritdoc />
        /// <exception cref="AggregateNotFoundException">
        /// Thrown when the provided <see cref="IAggregate"/>'s ID is not found as a key in the <see cref="ConcurrentDictionary{TKey,TValue}"/> used for persistence.
        /// </exception>
        public void Save(IAggregate aggregateToSave)
        {
            var newEvents = aggregateToSave.GetUncommittedEvents().Cast<object>().ToList();
            var originalVersion = aggregateToSave.Version - newEvents.Count;

            if (originalVersion == 0)
            {
                if (!EventStore.TryAdd(aggregateToSave.Id, new List<object>()))
                {
                    throw new AggregateVersionException();
                }
            }

            if (EventStore.TryGetValue(aggregateToSave.Id, out var theEvents))
            {
                if (theEvents.Count != originalVersion)
                {
                    throw new AggregateVersionException();
                }

                theEvents.AddRange(newEvents);
                aggregateToSave.ClearUncommittedEvents();
            }
            else
            {
                throw new AggregateNotFoundException();
            }
        }

        /// <inheritdoc />
        public T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue)
            where T : IAggregate
        {
            if (version <= 0)
            {
                throw new ArgumentException("Version must be greater than 0");
            }

            if (!EventStore.TryGetValue(aggregateId, out var theEvents))
            {
                throw new AggregateNotFoundException();
            }

            if (version != int.MaxValue && version > theEvents.Count)
            {
                throw new AggregateVersionException();
            }

            return BuildAggregate<T>(theEvents.Take(version));
        }

        private static T BuildAggregate<T>(IEnumerable<object> events)
            where T : IAggregate
        {
            var instance = (T)Activator.CreateInstance(typeof(T), true);
            foreach (var @event in events)
            {
                instance.ApplyEvent(@event);
            }

            return instance;
        }
    }
}
