using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Common.Log;
using EventStore.Core;
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using EventStore.Core.Services.Monitoring;
using EventStore.Core.Settings;
using EventStore.Core.TransactionLog.Checkpoint;
using EventStore.Core.TransactionLog.Chunks;
using EventStore.Core.TransactionLog.FileNamingStrategy;
using EventStore.Core.Util;

namespace CR.AggregateRepository.Tests
{
    public class EmbeddedEventStore
    {
        public readonly IPEndPoint TcpEndPoint;
        public readonly IPEndPoint HttpEndPoint;

        private string TfChunkFolderPath { get; set; }
        private SingleVNode _node { get; set; }

        public EmbeddedEventStore(string tfChunkFolderPath, int tcpPort, int httpPort)
        {
            TcpEndPoint = new IPEndPoint(IPAddress.Loopback, tcpPort);
            HttpEndPoint = new IPEndPoint(IPAddress.Loopback, httpPort);
            TfChunkFolderPath = tfChunkFolderPath;
        }

        public void Start()
        {
            _node = new SingleVNode(InitDb(), Settings(), false, 0xf4240, new ISubsystem[0]);
            var started = new ManualResetEvent(false);
            _node.MainBus.Subscribe(new AdHocHandler<SystemMessage.BecomeMaster>(m => started.Set()));
            _node.Start();
            started.WaitOne();
        }

        private SingleVNodeSettings Settings()
        {
            return new SingleVNodeSettings(TcpEndPoint, null, HttpEndPoint, new string[] { string.Format("http://{0}:{1}/", HttpEndPoint.Address, HttpEndPoint.Port) }, false, null, Opts.WorkerThreadsDefault,
                                           Opts.MinFlushDelayMsDefault,
                                           TimeSpan.FromMilliseconds(Opts.PrepareTimeoutMsDefault),
                                           TimeSpan.FromMilliseconds(Opts.CommitTimeoutMsDefault),
                                           TimeSpan.FromMilliseconds(Opts.StatsPeriodDefault), StatsStorage.None);
        }

        private TFChunkDb InitDb()
        {
            return new TFChunkDb(new TFChunkDbConfig(TfChunkFolderPath,
                                                    new VersionedPatternFileNamingStrategy(TfChunkFolderPath, "chunk-"),
                                                    1024,
                                                    0,
                                                    new InMemoryCheckpoint(), 
                                                    new InMemoryCheckpoint(),
                                                    new InMemoryCheckpoint(-1),
                                                    new InMemoryCheckpoint(-1)));
        }

    }
}
