using System;
using System.Collections;
using System.Collections.Generic;

namespace CR.AggregateRepository.Core
{
    public abstract class AggregateBase : IAggregate
    {
        public abstract object Id { get; }
        public int Version { get; private set; }

        private readonly List<Object> _changes = new List<Object>();

        private EventMap _map;
        protected abstract EventMap Map { get; }

        public AggregateBase()
        {
            _map = Map;
        }

        protected static Action<object> Noop = e => { };

        protected void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            _changes.Add(@event);
        }

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
                    string.Format("{0} can not be applied to {1}. No appropriate mapping is registered",
                        @event.GetType().Name,
                        GetType().Name));
            }
        }

        public ICollection GetUncommittedEvents()
        {
            return _changes;
        }

        public void ClearUncommittedEvents()
        {
            _changes.Clear();
        }
    }
}