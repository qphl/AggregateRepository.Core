// <copyright file="EventStoreAggregateRepository.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CR.AggregateRepository.Persistence.EventStore
{
    using System;
    using System.Linq;
    using System.Text;
    using Core;
    using Core.Exceptions;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// EventStore implementation of the Aggregate Repository.
    /// </summary>
    public class EventStoreAggregateRepository : IAggregateRepository
    {
        private const int ReadPageSize = 1000;

        private readonly IEventStoreConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStoreAggregateRepository"/> class.
        /// </summary>
        /// <param name="connection">EventStore connection.</param>
        public EventStoreAggregateRepository(IEventStoreConnection connection)
        {
            _connection = connection;
        }

        /// <inheritdoc />
        public void Save(IAggregate aggregateToSave)
        {
            var newEvents = aggregateToSave.GetUncommittedEvents().Cast<object>().ToList();
            var originalVersion = aggregateToSave.Version - newEvents.Count;

            var streamName = StreamNameForAggregateId(aggregateToSave.Id);
            var expectedVersion = originalVersion == 0 ? ExpectedVersion.NoStream : originalVersion - 1;
            var eventsToSave = newEvents.Select(e => ToEventData(Guid.NewGuid(), e)).ToList();

            try
            {
                _connection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave).Wait();
                aggregateToSave.ClearUncommittedEvents();
            }
            catch (StreamDeletedException ex)
            {
                throw new AggregateNotFoundException("Aggregate not found, stream deleted", ex);
            }
            catch (AggregateException ex)
            {
                var exceptions = ex.InnerExceptions;
                if (exceptions.Count == 1 && exceptions[0].GetType() == typeof(WrongExpectedVersionException))
                {
                    throw new AggregateVersionException("Aggregate version incorrect", ex);
                }

                throw;
            }
        }

        /// <inheritdoc />
        public T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue)
            where T : IAggregate
        {
            if (version <= 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            var streamName = StreamNameForAggregateId(aggregateId);
            var aggregate = (T)Activator.CreateInstance(typeof(T), true);

            long sliceStart = 0;
            var eventsCount = 0;
            StreamEventsSlice currentSlice;
            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                                    ? ReadPageSize
                                    : version - sliceStart + 1;

                currentSlice = _connection.ReadStreamEventsForwardAsync(streamName, sliceStart, (int)sliceCount, false).Result;

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                {
                    throw new AggregateNotFoundException();
                }

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                {
                    throw new AggregateNotFoundException();
                }

                sliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                {
                    aggregate.ApplyEvent(DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data));
                }

                eventsCount += currentSlice.Events.Length;
            }
            while (version >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            // if version is greater than number of events, throw exception
            if (eventsCount < version && version != int.MaxValue)
            {
                throw new AggregateVersionException("version is higher than actual version");
            }

            return aggregate;
        }

        private static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property("ClrType").Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }

        private static string StreamNameForAggregateId(object id)
        {
            return "aggregate-" + id;
        }

        private static EventData ToEventData(Guid eventId, object evnt)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt));

            var eventHeaders = new
                {
                    ClrType = evnt.GetType().AssemblyQualifiedName,
                };

            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders));
            var typeName = evnt.GetType().Name;

            return new EventData(eventId, typeName, true, data, metadata);
        }
    }
}
