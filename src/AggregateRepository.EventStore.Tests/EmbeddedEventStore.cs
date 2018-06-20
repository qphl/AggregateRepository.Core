// <copyright file="EmbeddedEventStore.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.EventStore.Tests
{
    extern alias EventStoreNetFrameworkClient;

    using System.Threading;
    using EventStoreNetFrameworkClient::EventStore.ClientAPI.Embedded;
    using EventStoreNetFrameworkClient::EventStore.Core;
    using EventStoreNetFrameworkClient::EventStore.Core.Bus;
    using EventStoreNetFrameworkClient::EventStore.Core.Messages;
    using EventStoreNetFrameworkClient::EventStore.Core.Services.Monitoring;

    public class EmbeddedEventStore
    {
        public EmbeddedEventStore(int tcpPort, int httpPort)
        {
            TcpEndPoint = tcpPort;
            HttpEndPoint = httpPort;
        }

        public ClusterVNode Node { get; private set; }

        public int HttpEndPoint { get; }

        public int TcpEndPoint { get; }

        public void Start()
        {
            Node = EmbeddedVNodeBuilder.AsSingleNode().RunInMemory()
                .AdvertiseExternalTCPPortAs(TcpEndPoint)
                .AdvertiseExternalHttpPortAs(HttpEndPoint)
                .AddExternalHttpPrefix($"http://127.0.0.1:{HttpEndPoint}/").
                WithStatsStorage(StatsStorage.None).Build();
            var started = new ManualResetEvent(false);

            Node.MainBus.Subscribe(new AdHocHandler<SystemMessage.BecomeMaster>(m => started.Set()));
            Node.Start();

            started.WaitOne();
        }

        public void Stop()
        {
            var stopped = new ManualResetEvent(false);
            Node.MainBus.Subscribe(new AdHocHandler<SystemMessage.BecomeShutdown>(m => stopped.Set()));

            Node?.Stop();
            stopped.WaitOne();
        }
    }
}
