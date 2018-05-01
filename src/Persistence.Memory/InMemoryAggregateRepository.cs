// <copyright file="InMemoryAggregateRepository.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Persistence.Memory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// In Memory implementation of the Aggregate Repository.
    /// </summary>
    public class InMemoryAggregateRepository : IAggregateRepository
    {
        /// <summary>
        /// In memory event storage.
        /// </summary>
#pragma warning disable SA1401 // Fields should be private
        public readonly ConcurrentDictionary<object, List<object>> EventStore = new ConcurrentDictionary<object, List<object>>();
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryAggregateRepository"/> class.
        /// </summary>
        public InMemoryAggregateRepository()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryAggregateRepository"/> class.
        /// </summary>
        /// <param name="initialEvents">A dictionary of initial events added when itializing the instance.</param>
        public InMemoryAggregateRepository(Dictionary<object, List<object>> initialEvents)
        {
            foreach (var item in initialEvents)
            {
                EventStore.TryAdd(item.Key, item.Value);
            }
        }

        /// <inheritdoc />
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

            if (EventStore.TryGetValue(aggregateId, out var theEvents))
            {
                if (version != int.MaxValue && version > theEvents.Count)
                {
                    throw new AggregateVersionException();
                }

                // return theEvents.Take(version);
                return BuildAggregate<T>(theEvents.Take(version));
            }
            else
            {
                throw new AggregateNotFoundException();
            }
        }

        private T BuildAggregate<T>(IEnumerable<object> events)
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
