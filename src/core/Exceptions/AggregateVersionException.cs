using System;
using System.Runtime.Serialization;

namespace CR.AggregateRepository.Core.Exceptions
{
    [Serializable]
    public class AggregateVersionException : Exception
    {

        public AggregateVersionException()
        {
        }

        public AggregateVersionException(string message) : base(message)
        {
        }

        public AggregateVersionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AggregateVersionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}