using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using PoolManager.Monitor.Extensions;
using PoolManager.Monitor.Interfaces;
using PoolManager.Monitor.Models;
using PoolManager.Monitor.Orphans;

namespace PoolManager.Monitor
{
    public sealed class PoolManagerMonitor
    {
        private const int MinutesBetweenMonitorRuns = 1000 * 60 * 2;
        private readonly TelemetryClient _telemetryClient;
        private readonly IEnumerable<IMonitor> _monitors;

        public PoolManagerMonitor(TelemetryClient telemetryClient, IEnumerable<IMonitor> monitors)
        {
            _telemetryClient = telemetryClient;
            _monitors = monitors.ToList();
        }

        internal async Task StartAsync(CancellationToken cancellationToken, BlockingCollection<ICommand> commands)
        {
            _telemetryClient.TrackStateChangeEvents("PoolManagerMonitor", "Active");
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var monitor in _monitors)
                {
                    try
                    {
                        var newCommands = (await monitor.RunAsync(cancellationToken)).ToList();
                        if (newCommands.Any())
                        {
                            newCommands.ForEach(x => commands.Add(x, cancellationToken));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception and continue removing orphans
                        _telemetryClient.TrackException(ex, new Dictionary<string, string> { { "Command", monitor.Name } });
                    }

                    await Task.Delay(MinutesBetweenMonitorRuns, cancellationToken);
                }
            }

            _telemetryClient.TrackStateChangeEvents("PoolManagerMonitor", "Deactive");
        }

        internal async Task<IDictionary<string, IEnumerable<OrphanInfo>>> GetAllOrphansAsync(CancellationToken cancellationToken)
        {
            var monitor = (OrphanMonitor)_monitors.Single(m => m.Name.Equals(OrphanMonitor.OrphanMonitorName));
            return await monitor.GetAllOrphansAsync(cancellationToken);
        }
    }
}