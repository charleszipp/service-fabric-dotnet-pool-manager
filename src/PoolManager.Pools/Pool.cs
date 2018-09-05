﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
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
        private const string CleanupRemovedInstancesReminderKey = "cleanup-removed-instances";

        public Pool(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient, IActorProxyFactory actorProxyFactory)
            : base(actorService, actorId)
        {
            _context = new PoolContext(
                actorId.GetStringId(),
                new PoolStateProvider(new PoolStateIdle(), new PoolStateActive()),
                new InstanceProxy(actorProxyFactory),
                StateManager,
                telemetryClient
            );
            _telemetryClient = telemetryClient;
        }
        public async Task StartAsync(StartPoolRequest request)
        {
            await _context.StartAsync(request);
            await SetReminderAsync(EnsurePoolSizeReminderKey, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            var cleanupInterval = request.ExpirationQuanta.Add(TimeSpan.FromMilliseconds((int)request.ExpirationQuanta.TotalMilliseconds * 0.05));
            await SetReminderAsync(CleanupRemovedInstancesReminderKey, null, cleanupInterval, cleanupInterval);
        }   
        
        public async Task StopAsync()
        {
            await _context.StopAsync();
            await UnregisterReminderAsync(EnsurePoolSizeReminderKey);
            await UnregisterReminderAsync(CleanupRemovedInstancesReminderKey);
        }

        public Task<GetInstanceResponse> GetAsync(GetInstanceRequest request) => _context.GetAsync(request);

        public Task VacateInstanceAsync(VacateInstanceRequest request) => _context.VacateInstanceAsync(request);

        public async Task<ConfigurationResponse> GetConfigurationAsync()
        {
            var config = await _context.GetPoolConfigurationAsync();
            return new ConfigurationResponse(config.ExpirationQuanta, config.HasPersistedState,
                config.IdleServicesPoolSize, config.IsServiceStateful, config.MaxPoolSize,
                config.MinReplicaSetSize, config.PartitionScheme, config.ServicesAllocationBlockSize,
                config.ServiceTypeUri, config.TargetReplicasetSize);
        }

        public Task<bool> IsActive() => Task.FromResult(_context.CurrentState == PoolStates.Active);

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
                        case CleanupRemovedInstancesReminderKey:
                            await _context.CleanupRemovedInstancesAsync();
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