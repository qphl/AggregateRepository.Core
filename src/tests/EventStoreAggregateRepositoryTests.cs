using System;
using System.Collections.Generic;
using System.IO;
using CR.AggregateRepository.Persistance.EventStore;
using EventStore.ClientAPI;

namespace CR.AggregateRepository.Tests
{
    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected override void InitRepository()
        {
            var db = new EmbeddedEventStore(GetTemporaryDirectory(), 11113, 12113);
            db.Start();

            var testEventStoreConnection = EventStoreConnection.Create(db.TcpEndPoint);
            testEventStoreConnection.Connect();

            var events = new List<EventData>();
            for (int i = 0; i < 100; i++)
            {
                var e = new EventData(Guid.NewGuid(), "woftamevent", false, new byte[]{}, new byte[]{});
                events.Add(e);
            }

            testEventStoreConnection.AppendToStream("woftamStream", ExpectedVersion.Any, events);
            var slice = testEventStoreConnection.ReadStreamEventsForward("woftamStream", 0, 1000, false);

            _repoUnderTest = new EventStoreAggregateRepository(testEventStoreConnection);
        }

        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}