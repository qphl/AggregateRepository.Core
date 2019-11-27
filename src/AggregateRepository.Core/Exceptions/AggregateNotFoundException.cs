// <copyright file="AggregateNotFoundException.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <inheritdoc />
    /// <summary>
    /// Thrown in the event of an aggregate not being found.
    /// </summary>
    [Serializable]
    public class AggregateNotFoundException : Exception
    {
#pragma warning disable SA1648 // inheritdoc should be used with inheriting class
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotFoundException"/> class with no message, or inner exception.
        /// </summary>
        public AggregateNotFoundException()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotFoundException"/> class with the provided message.
        /// </summary>
        /// <param name="message">A brief explanation about the cause of the exception.</param>
        // ReSharper disable once UnusedMember.Global
        public AggregateNotFoundException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotFoundException"/> class with the provided message and inner exception.
        /// </summary>
        /// <param name="message">A brief explanation about the cause of the exception.</param>
        /// <param name="inner">The exception which resulted in this exception being thrown.</param>
        public AggregateNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotFoundException"/> class with serialized data from the provided <see cref="SerializationInfo"/> and <see cref="StreamingContext"/>.
        /// </summary>
        /// <param name="info">The serialization information to deserialize the data with.</param>
        /// <param name="context">The streaming context to get the serialized data from.</param>
        protected AggregateNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore SA1648 // inheritdoc should be used with inheriting class
    }
}
