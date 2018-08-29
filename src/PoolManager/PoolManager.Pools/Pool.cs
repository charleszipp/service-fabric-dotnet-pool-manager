﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Pool : Actor, IPool, IRemindable
    {
        private readonly PoolContext _context;
        private readonly TelemetryClient _telemetryClient;

        public Pool(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient)
            : base(actorService, actorId)
        {
            _context = new PoolContext(
                actorId.GetStringId(),
                new PoolStateProvider(new PoolStateIdle(), new PoolStateActive()),
                new InstanceProxy(
                    new CorrelatingActorProxyFactory(ActorService.Context, callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient: callbackClient))
                ),
                StateManager,
                telemetryClient
                );
            _telemetryClient = telemetryClient;
        }

        public async Task StartAsync(StartPoolRequest request)
        {
            await _context.StartAsync(request);
            try
            {
                var reminder = GetReminder("ensure-pool-size");
                await UnregisterReminderAsync(reminder);
            }
            catch (ReminderNotFoundException)
            {
            }
            await RegisterReminderAsync("ensure-pool-size", null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(1));
        }        

        public Task GetAsync(GetInstanceRequest request) => _context.GetAsync(request);

        public Task VacateInstanceAsync(VacateInstanceRequest request) => _context.VacateInstanceAsync(request);

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
                        case "ensure-pool-size":
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
    }
}