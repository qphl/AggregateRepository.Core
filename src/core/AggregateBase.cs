using System;
using System.Collections;
using System.Collections.Generic;
using ReflectionMagic;

namespace CR.AggregateRepository.Core
{
    public abstract class AggregateBase : IAggregate
    {
        public abstract object Id { get; }
        public int Version { get; private set; }

        private readonly List<Object> _changes = new List<Object>();
        
        protected void RaiseEvent(object @event)
        {
            ApplyEvent(@event);
            _changes.Add(@event);
        }

        public void ApplyEvent(object @event)
        {
            this.AsDynamic().Apply(@event);
            Version++;
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