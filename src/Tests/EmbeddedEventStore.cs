using System.Threading;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using EventStore.Core.Services.Monitoring;

namespace CR.AggregateRepository.Tests
{
    public class EmbeddedEventStore
    {
        public ClusterVNode Node { get; private set; }

        public int HttpEndPoint { get; }
        public int TcpEndPoint { get; }

        public EmbeddedEventStore(int tcpPort, int httpPort)
        {
            TcpEndPoint = tcpPort;
            HttpEndPoint = httpPort;
        }

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
