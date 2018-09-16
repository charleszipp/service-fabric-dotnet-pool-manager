using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    [StatePersistence(StatePersistence.Persisted)]
    public class Pool : Actor, IPool, IRemindable
    {
        private readonly PoolContext _context;
        private readonly TelemetryClient _telemetryClient;
        private const string EnsurePoolSizeReminderKey = "ensure-pool-size";
        public Pool(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient, IInstanceProxy instanceProxy)
            : base(actorService, actorId)
        {
            _context = new PoolContext(actorId.GetStringId(), new PoolStateProvider(new PoolStateIdle(), new PoolStateActive()),
                instanceProxy, StateManager, telemetryClient);
            _telemetryClient = telemetryClient;
        }
        public async Task StartAsync(StartPoolRequest request)
        {
            await _context.StartAsync(request);
            await SetReminderAsync(EnsurePoolSizeReminderKey, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        }   

        public async Task<ConfigurationResponse> GetConfigurationAsync()
        {
            var config = await _context.GetPoolConfigurationAsync();
            return new ConfigurationResponse(config.ExpirationQuanta, config.HasPersistedState,
                config.IdleServicesPoolSize, config.IsServiceStateful, config.MaxPoolSize,
                config.MinReplicaSetSize, config.PartitionScheme, config.ServicesAllocationBlockSize,
                config.ServiceTypeUri, config.TargetReplicasetSize);
        }

        protected override Task OnActivateAsync() => _context.ActivateAsync();

        protected override Task OnDeactivateAsync() => _context.DeactivateAsync();

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            using (var request = _telemetryClient.StartOperation<RequestTelemetry>(reminderName))
            {
                request.Telemetry.Properties.Add("DueTime", dueTime.ToString());
                request.Telemetry.Properties.Add("Interval", period.ToString());
                try
                {
                    switch (reminderName)
                    {
                        case EnsurePoolSizeReminderKey:
                            await _context.EnsurePoolSizeAsync();
                            break;
                    }
                }
                catch(Exception ex)
                {
                    request.Telemetry.Success = false;
                    _telemetryClient.TrackException(ex);
                }
            }            
        }

        private async Task UnregisterReminderAsync(string name)
        {
            try
            {
                var reminder = GetReminder(name);
                await UnregisterReminderAsync(reminder);
            }
            catch (ReminderNotFoundException)
            {
            }
        }

        private async Task SetReminderAsync(string name, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            await UnregisterReminderAsync(name);
            await RegisterReminderAsync(name, state, dueTime, period);
        }
    }
}