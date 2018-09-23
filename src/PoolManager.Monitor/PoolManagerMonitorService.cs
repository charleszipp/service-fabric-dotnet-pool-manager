using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PoolManager.Monitor.Extensions;
using PoolManager.Monitor.Interfaces;
using PoolManager.Monitor.Models;

namespace PoolManager.Monitor
{
    public sealed class PoolManagerMonitorService : StatelessService, IPoolManagerMonitorService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly PoolManagerMonitor _monitor;
        private Thread _commandWorkerThread;
        private readonly BlockingCollection<ICommand> _commands = new BlockingCollection<ICommand>();
        private readonly PoolManagerCommandWorker _commandWorker;
        private CancellationToken _cancellationToken;

        public PoolManagerMonitorService(StatelessServiceContext serviceContext, TelemetryClient telemetryClient, PoolManagerMonitor monitor, PoolManagerCommandWorker commandWorker)
            : base(serviceContext)
        {
            _telemetryClient = telemetryClient;
            _monitor = monitor;
            _commandWorker = commandWorker;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                new ServiceInstanceListener(context => new FabricTransportServiceRemotingListener(context, this))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _telemetryClient.TrackStateChangeEvents("PoolManagerMonitorService", "Active");
            _cancellationToken = cancellationToken;
            
            // Start the worker
            _commandWorkerThread = new Thread(() => _commandWorker.Run(_cancellationToken, _commands));
            _commandWorkerThread.Start();

            await _monitor.StartAsync(_cancellationToken, _commands);

            cancellationToken.Register(() => {_telemetryClient.TrackStateChangeEvents("PoolManagerMonitorService", "Deactive");});
        }

        public Task<IDictionary<string, IEnumerable<OrphanInfo>>> GetOrphansAsync() => _monitor.GetAllOrphansAsync(_cancellationToken);
    }
}