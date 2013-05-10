using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CR.AggregateRepository.Core.Exceptions
{
    [Serializable]
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException()
        {
        }

        public AggregateNotFoundException(string message) : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AggregateNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

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
