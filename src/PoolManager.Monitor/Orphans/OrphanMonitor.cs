using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using PoolManager.Monitor.Interfaces;
using PoolManager.Monitor.Models;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.Monitor.Orphans
{
    public class OrphanMonitor : IMonitor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly FabricClient _fabricClient;
        private readonly IRemoveOrphanCommandFactory _orphanFactory;
        private readonly IPoolProxy _poolProxy;
        private const int TotalTimesDetachedBeforeFlaggedAsOrphan = 2;
        public static string OrphanMonitorName = "Orphan Monitor";

        // This dictionary stores the detached services, including number of times seen, that service was detected as being detached, not tracked,
        // from the pool manager.  This is needed to prevent this service from removing a service that was just created by the pool manager,
        // but not yet stored in its collection.
        private readonly Dictionary<string,  Dictionary<Uri, int>> _detachedServices = new Dictionary<string, Dictionary<Uri, int>>();

        public string Name { get; } = OrphanMonitorName;

        public OrphanMonitor(TelemetryClient telemetryClient, FabricClient fabricClient, IRemoveOrphanCommandFactory orphanFactory, IPoolProxy poolProxy)
        {
            _telemetryClient = telemetryClient;
            _fabricClient = fabricClient;
            _orphanFactory = orphanFactory;
            _poolProxy = poolProxy;
        }
   
        public async Task<IEnumerable<ICommand>> RunAsync(CancellationToken cancellationToken)
        {
            var commands = new List<ICommand>();

            var usages = (await _poolProxy.GetInstancesAsync(cancellationToken)).ToList();

            if (cancellationToken.IsCancellationRequested || !usages.Any()) return commands;

            var orphansByType = await GetOrphansForManagedTypesAsync(usages, true, TotalTimesDetachedBeforeFlaggedAsOrphan);

            if (cancellationToken.IsCancellationRequested || !orphansByType.Any()) return commands;

            foreach (var obt in orphansByType)
            {
                var orphans = obt.Value.ToList();
                var serviceType = obt.Key;

                if (!orphans.Any())
                {
                    continue;
                }

                var properties = new Dictionary<string, string>
                {
                    {"EventName", $"{RemoveOrphanCommand.OrphanEventPrefix} reported {orphans.Count} orphan(s) found for service type uri {serviceType}."},
                    {"ServiceTypeUri", serviceType}
                };
                _telemetryClient.TrackEvent($"{RemoveOrphanCommand.OrphanEventPrefix} reported warning for service type uri {serviceType}.", properties);
                _telemetryClient.TrackMetric("Orphaned-Service-Count", orphans.Count, new Dictionary<string, string> { { "ServiceTypeUri", serviceType } });

                // For each orphan schedule an action to remove it and put this action in the actions collection
                orphans.ForEach(x => commands.Add(_orphanFactory.CreateRemoveOrphanCommand(x)));
            }

            return commands;
        }

        public async Task<IDictionary<string, IEnumerable<OrphanInfo>>> GetAllOrphansAsync(CancellationToken cancellationToken)
        {
            var usages = (await _poolProxy.GetInstancesAsync(cancellationToken)).ToList();

            if (cancellationToken.IsCancellationRequested || !usages.Any()) return new Dictionary<string, IEnumerable<OrphanInfo>>();

            // This will return any service instance that could possible be an orphan.  It can contain false positives.
            return await GetOrphansForManagedTypesAsync(usages, false, 1);
        }

        private async Task<IDictionary<string, IEnumerable<OrphanInfo>>> GetOrphansForManagedTypesAsync(IEnumerable<GetInstancesResponse> usages, bool incrementCount, int timesSeenBeforeOrphaned)
        {
            var applicationList = await _fabricClient.QueryManager.GetApplicationListAsync();

            return (await Task.WhenAll(usages.Select(usage => DetectDetachedServices(applicationList, usage, incrementCount, TotalTimesDetachedBeforeFlaggedAsOrphan))))
                .SelectMany(d => d).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private async Task<IDictionary<string, IEnumerable<OrphanInfo>>> DetectDetachedServices(ApplicationList applicationList, GetInstancesResponse usage, bool incrementCount, int timesSeenBeforeOrphaned)
        {
            var detachedServices = (await GetServicesAsync(applicationList, usage))
                .Where(s => usage.VacantInstances.All(i => s.ServiceName.AbsoluteUri.EndsWith(i.ToString()) == false))
                .Where(s => usage.OccupiedInstances.All(i => s.ServiceName.AbsoluteUri.EndsWith(i.ToString()) == false))
                .ToList();

            var possibleOrphans = _detachedServices.ContainsKey(usage.ServiceTypeUri) ? _detachedServices[usage.ServiceTypeUri] : new Dictionary<Uri, int>();

            // First remove all previously detected detached services that are no longer detached for the service type
            possibleOrphans.Keys.Except(detachedServices.Select(x => x.ServiceName)).ToList().ForEach(key => possibleOrphans.Remove(key));

            if (incrementCount && possibleOrphans.Any()) possibleOrphans.Keys.ToList().ForEach(x => possibleOrphans[x] += 1);

            // Add in newly detected detached services
            detachedServices.Select(x => x.ServiceName).Except(possibleOrphans.Keys).ToList().ForEach(name => possibleOrphans.Add(name, 1));

            _detachedServices[usage.ServiceTypeUri] = possibleOrphans;

            return new Dictionary<string, IEnumerable<OrphanInfo>>
            {
                {
                    usage.ServiceTypeUri,
                    detachedServices.Where(s => possibleOrphans[s.ServiceName] >= timesSeenBeforeOrphaned)
                        .Select(s => new OrphanInfo(s.ServiceName.AbsoluteUri, usage.ServiceTypeUri, s.HealthState, s.ServiceStatus))
                }
            };
        }

        private async Task<IEnumerable<Service>> GetServicesAsync(ApplicationList applicationList, GetInstancesResponse usage)
        {
            var app = applicationList.SingleOrDefault(a =>
            {
                var appName = $"{a.ApplicationName.AbsoluteUri}/";
                return usage.ServiceTypeUri.StartsWith(appName);
            });

            if (app == null)
            {
                return new List<Service>();
            }

            return (await _fabricClient.QueryManager.GetServiceListAsync(app.ApplicationName))
                .Where(g => g.ServiceName.AbsoluteUri.Contains(usage.ServiceTypeUri))
                .ToList();
        }
    }
}