using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CR.AggregateRepository.Core;
using CR.AggregateRepository.Core.Exceptions;

namespace CR.AggregateRepository.Persistance.Memory
{
    public class InMemoryAggregateRepository : IAggregateRepository
    {
        public readonly ConcurrentDictionary<object,List<object>> EventStore = new ConcurrentDictionary<object, List<object>>();
        
        public InMemoryAggregateRepository()
        {
            
        }

        public InMemoryAggregateRepository(Dictionary<object, List<object>> initialEvents)
        {
            foreach (var item in initialEvents)
            {
                EventStore.TryAdd(item.Key, item.Value);
            }
        }

        public void Save(IAggregate aggregateToSave)
        {
            var newEvents = aggregateToSave.GetUncommittedEvents().Cast<object>().ToList();
            var originalVersion = aggregateToSave.Version - newEvents.Count;

            List<object> theEvents;

            if (originalVersion == 0)
            {
                if (!EventStore.TryAdd(aggregateToSave.Id, new List<object>()))
                {
                    throw new AggregateVersionException();
                }
            }

            if (EventStore.TryGetValue(aggregateToSave.Id, out theEvents))
            {
                if(theEvents.Count != originalVersion)
                    throw new AggregateVersionException();

                theEvents.AddRange(newEvents);
                aggregateToSave.ClearUncommittedEvents();
            }
            else
            {
               throw new AggregateNotFoundException();
            }
        }

        public T GetAggregateFromRepository<T>(object aggregateId, int version) where T : IAggregate
        {
            if(version <= 0)
                throw new ArgumentException("Version must be greater than 0");

            List<object> theEvents;
            if (EventStore.TryGetValue(aggregateId, out theEvents))
            {
                if (version != Int32.MaxValue && version > theEvents.Count)
                    throw new AggregateVersionException();

             //   return theEvents.Take(version);
                return BuildAggregate<T>(theEvents.Take(version));
            }
            else
            {
                throw new AggregateNotFoundException();
            }
        }

        private T BuildAggregate<T>(IEnumerable<object> events) where T : IAggregate
        {
            var instance =  (T)Activator.CreateInstance(typeof(T), true);
            foreach (var @event in events)
            {
                instance.ApplyEvent(@event);
            }
            return instance;
        }

        public T GetAggregateFromRepository<T>(object aggregateId) where T : IAggregate
        {
            return GetAggregateFromRepository<T>(aggregateId, Int32.MaxValue);
        }
    }
}
 