using System;
using System.Collections.Generic;
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
}
