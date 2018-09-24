using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using PoolManager.Monitor.Interfaces;
using PoolManager.Monitor.Models;

namespace PoolManager.Monitor.Orphans
{
    public class RemoveOrphanCommand : ICommand
    {
        public static string OrphanEventPrefix = "Orphan Monitoring";
        private readonly TelemetryClient _telemetryClient;
        private readonly FabricClient _fabricClient;
        private readonly OrphanInfo _orphan;
        public string Name { get; } = "Remove Orphan Command";


        public RemoveOrphanCommand(TelemetryClient telemetryClient, FabricClient fabricClient, OrphanInfo orphan)
        {
            _telemetryClient = telemetryClient;
            _fabricClient = fabricClient;
            _orphan = orphan;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var ds = new DeleteServiceDescription(new Uri(_orphan.ServiceName, UriKind.RelativeOrAbsolute))
                {
                    ForceDelete = true
                };
                await _fabricClient.ServiceManager.DeleteServiceAsync(ds).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested) return;
                var properties = new Dictionary<string, string>
                {
                    {"EventName", $"{OrphanEventPrefix} successfully removed orphaned service {_orphan}."},
                    {"ServiceName", _orphan.ServiceName},
                    {"ServiceTypeUri", _orphan.ServiceTypeUri}
                };
                _telemetryClient.TrackEvent($"{OrphanEventPrefix} removed orphaned service {_orphan}.", properties);
            }
            catch (Exception ex)
            {
                // Log the exception and continue removing orphans
                var properties = new Dictionary<string, string>
                {
                    {"ServiceName", _orphan.ServiceName},
                    {"ServiceTypeUri", _orphan.ServiceTypeUri}
                };
                _telemetryClient.TrackException(ex, properties);
            }
        }
    }
}