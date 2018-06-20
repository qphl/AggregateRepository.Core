// <copyright file="EventStoreAggregateRepositoryTests.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.EventStore.Tests
{
    extern alias EventStoreNetFrameworkClient;
    extern alias EventStoreNetCoreClient;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CR.AggregateRepository.EventStore;
    using EventStoreNetCoreClient::EventStore.ClientAPI;
    using EventStoreNetFrameworkClient::EventStore.ClientAPI.Embedded;
    using EventStoreNetFrameworkClient::EventStore.ClientAPI.Exceptions;
    using SliceReadStatus = EventStoreNetFrameworkClient::EventStore.ClientAPI.SliceReadStatus;

    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private EventStoreNetFrameworkClient::EventStore.ClientAPI.IEventStoreConnection _connection;
        private EmbeddedEventStore _eventStore;

        protected override void InitRepository()
        {
            _eventStore = new EmbeddedEventStore(11113, 12113);
            _eventStore.Start();

            _connection = EmbeddedEventStoreConnection.Create(_eventStore.Node);
            _connection.ConnectAsync().Wait();

            RepoUnderTest = new EventStoreAggregateRepository(new ConnectionTranslator(_connection));
        }

        protected override void CleanUpRepository()
        {
            _connection.Close();
            _eventStore.Stop();
        }

        /// <summary>
        /// This class is an awful hack which allows us to use an event store embedded client to test code 
        /// expecting the .net standard client.
        /// This is necessary, because the embedded client does not support .net standard, and pulls in the full
        /// framework client api, which creates conflicts
        /// We only implement the two methods actually needed by aggregate repository, and have to resort to
        /// horrible reflection field setting and System.Runtime.Serialization.FormatterServices.GetUninitializedObject
        /// to create instances of objects with only internal constructors and set their readonly fields.
        /// </summary>
        private class ConnectionTranslator : IEventStoreConnection
        {
            private readonly EventStoreNetFrameworkClient::EventStore.ClientAPI.IEventStoreConnection _connection;

            public ConnectionTranslator(EventStoreNetFrameworkClient.EventStore.ClientAPI.IEventStoreConnection connection) => _connection = connection;

            #region Not Implemented Events

            event EventHandler<ClientConnectionEventArgs> IEventStoreConnection.Connected
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            event EventHandler<ClientConnectionEventArgs> IEventStoreConnection.Disconnected
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            event EventHandler<ClientReconnectingEventArgs> IEventStoreConnection.Reconnecting
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            event EventHandler<ClientClosedEventArgs> IEventStoreConnection.Closed
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            event EventHandler<ClientErrorEventArgs> IEventStoreConnection.ErrorOccurred
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }

            event EventHandler<ClientAuthenticationFailedEventArgs> IEventStoreConnection.AuthenticationFailed
            {
                add => throw new NotImplementedException();
                remove => throw new NotImplementedException();
            }
            #endregion

            // ReSharper disable UnassignedGetOnlyAutoProperty
            public string ConnectionName { get; }

            public ConnectionSettings Settings { get; } // ReSharper restore UnassignedGetOnlyAutoProperty

            public async Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null)
            {
                var translatedEvents = events.Select(e => new EventStoreNetFrameworkClient.EventStore.ClientAPI.EventData(e.EventId, e.Type, e.IsJson, e.Data, e.Metadata));
                var translatedUserCredentials = userCredentials == null ? null : new EventStoreNetFrameworkClient.EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);

                try
                {
                    var result = await _connection.AppendToStreamAsync(stream, expectedVersion, translatedEvents, translatedUserCredentials);
                    var logPosition = new Position(result.LogPosition.CommitPosition, result.LogPosition.PreparePosition);
                    return new WriteResult(result.NextExpectedVersion, logPosition);
                }
                catch (WrongExpectedVersionException e)
                {
                    throw new EventStoreNetCoreClient.EventStore.ClientAPI.Exceptions.WrongExpectedVersionException(e.Message);
                }
            }

            public async Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string stream, long start, int count, bool resolveLinkTos, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null)
            {
                var translatedUserCredentials = userCredentials == null ? null : new EventStoreNetFrameworkClient.EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);
                return await _connection.ReadStreamEventsForwardAsync(stream, start, count, resolveLinkTos, translatedUserCredentials).ContinueWith(t =>
                {
                    var result = t.Result;
                    var translatedStatus = EventStoreNetCoreClient.EventStore.ClientAPI.SliceReadStatus.StreamDeleted;

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (result.Status)
                    {
                        case SliceReadStatus.StreamNotFound:
                            translatedStatus = EventStoreNetCoreClient.EventStore.ClientAPI.SliceReadStatus.StreamNotFound;
                            break;
                        case SliceReadStatus.Success:
                            translatedStatus = EventStoreNetCoreClient.EventStore.ClientAPI.SliceReadStatus.Success;
                            break;
                    }

                    var translatedEvents = result.Events.Select(e =>
                    {
                        var recordedEvent = (RecordedEvent)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RecordedEvent));

                        recordedEvent.Created = e.Event.Created;
                        recordedEvent.CreatedEpoch = e.Event.CreatedEpoch;

                        var recordedType = recordedEvent.GetType();
                        recordedType.GetField("Data")?.SetValue(recordedEvent, e.Event.Data);
                        recordedType.GetField("EventStreamId")?.SetValue(recordedEvent, e.Event.EventStreamId);
                        recordedType.GetField("EventType")?.SetValue(recordedEvent, e.Event.EventType);
                        recordedType.GetField("Metadata")?.SetValue(recordedEvent, e.Event.Metadata);
                        recordedType.GetField("EventId")?.SetValue(recordedEvent, e.Event.EventId);
                        recordedType.GetField("EventNumber")?.SetValue(recordedEvent, e.Event.EventNumber);
                        recordedType.GetField("IsJson")?.SetValue(recordedEvent, e.Event.IsJson);

                        object resolvedEvent = (ResolvedEvent)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ResolvedEvent));

                        var resolvedType = resolvedEvent.GetType();
                        resolvedType.GetField("Event")?.SetValue(resolvedEvent, recordedEvent);
                        resolvedType.GetField("Link")?.SetValue(resolvedEvent, null);
                        resolvedType.GetField("OriginalEvent")?.SetValue(resolvedEvent, null);

                        return (ResolvedEvent)resolvedEvent;
                    });

                    var slice = (StreamEventsSlice)System.Runtime.Serialization.FormatterServices
                        .GetUninitializedObject(typeof(StreamEventsSlice));

                    var sliceType = slice.GetType();

                    sliceType.GetField("Status")?.SetValue(slice, translatedStatus);
                    sliceType.GetField("Events")?.SetValue(slice, translatedEvents.ToArray());
                    sliceType.GetField("FromEventNumber")?.SetValue(slice, result.FromEventNumber);
                    sliceType.GetField("LastEventNumber")?.SetValue(slice, result.LastEventNumber);
                    sliceType.GetField("NextEventNumber")?.SetValue(slice, result.NextEventNumber);
                    sliceType.GetField("Stream")?.SetValue(slice, result.Stream);
                    sliceType.GetField("IsEndOfStream")?.SetValue(slice, result.IsEndOfStream);
                    sliceType.GetField("ReadDirection")?.SetValue(slice, ReadDirection.Forward);

                    return slice;
                });
            }

            #region Not used by current aggregate repo implementation
            public void Dispose() => throw new NotImplementedException();

            public Task ConnectAsync() => throw new NotImplementedException();

            public void Close() => throw new NotImplementedException();

            public Task<DeleteResult> DeleteStreamAsync(string stream, long expectedVersion, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<DeleteResult> DeleteStreamAsync(string stream, long expectedVersion, bool hardDelete, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, params EventData[] events) => throw new NotImplementedException();

            public Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials, params EventData[] events) => throw new NotImplementedException();

            public Task<ConditionalWriteResult> ConditionalAppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<EventStoreTransaction> StartTransactionAsync(string stream, long expectedVersion, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public EventStoreTransaction ContinueTransaction(long transactionId, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<EventReadResult> ReadEventAsync(string stream, long eventNumber, bool resolveLinkTos, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<StreamEventsSlice> ReadStreamEventsBackwardAsync(string stream, long start, int count, bool resolveLinkTos, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<AllEventsSlice> ReadAllEventsForwardAsync(Position position, int maxCount, bool resolveLinkTos, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<AllEventsSlice> ReadAllEventsBackwardAsync(Position position, int maxCount, bool resolveLinkTos, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<EventStoreSubscription> SubscribeToStreamAsync(string stream, bool resolveLinkTos, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public EventStoreStreamCatchUpSubscription SubscribeToStreamFrom(string stream, long? lastCheckpoint, bool resolveLinkTos, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null, int readBatchSize = 500, string subscriptionName = "") => throw new NotImplementedException();

            public EventStoreStreamCatchUpSubscription SubscribeToStreamFrom(string stream, long? lastCheckpoint, CatchUpSubscriptionSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<EventStoreSubscription> SubscribeToAllAsync(bool resolveLinkTos, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public EventStorePersistentSubscriptionBase ConnectToPersistentSubscription(string stream, string groupName, Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null, int bufferSize = 10, bool autoAck = true) => throw new NotImplementedException();

            public Task<EventStorePersistentSubscriptionBase> ConnectToPersistentSubscriptionAsync(string stream, string groupName, Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null, int bufferSize = 10, bool autoAck = true) => throw new NotImplementedException();

            public EventStoreAllCatchUpSubscription SubscribeToAllFrom(Position? lastCheckpoint, bool resolveLinkTos, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null, int readBatchSize = 500, string subscriptionName = "") => throw new NotImplementedException();

            public EventStoreAllCatchUpSubscription SubscribeToAllFrom(Position? lastCheckpoint, CatchUpSubscriptionSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task UpdatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials credentials) => throw new NotImplementedException();

            public Task CreatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials credentials) => throw new NotImplementedException();

            public Task DeletePersistentSubscriptionAsync(string stream, string groupName, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<WriteResult> SetStreamMetadataAsync(string stream, long expectedMetastreamVersion, StreamMetadata metadata, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<WriteResult> SetStreamMetadataAsync(string stream, long expectedMetastreamVersion, byte[] metadata, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<StreamMetadataResult> GetStreamMetadataAsync(string stream, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task<RawStreamMetadataResult> GetStreamMetadataAsRawBytesAsync(string stream, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();

            public Task SetSystemSettingsAsync(SystemSettings settings, EventStoreNetCoreClient.EventStore.ClientAPI.SystemData.UserCredentials userCredentials = null) => throw new NotImplementedException();
            #endregion
        }
    }
}
