using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Ninject;
using PoolManager.Core;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    [StatePersistence(StatePersistence.Persisted)]
    public class Pool : Actor, IPool, IRemindable
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly Mediator _mediator;
        private readonly IKernel _kernel;
        private const string EnsurePoolSizeReminderKey = "ensure-pool-size";

        public Pool(
            ActorService actorService, 
            ActorId actorId, 
            IGuidGetter guidGetter = null,
            TelemetryClient telemetry = null,
            IActorProxyFactory actorProxyFactory = null,
            IServiceProxyFactory serviceProxyFactory = null)
            : base(actorService, actorId)
        {
            _kernel = new StandardKernel(new PoolActorModule(guidGetter))
                .WithCore(actorService.Context, StateManager, telemetry: telemetry, 
                    actorProxyFactory: actorProxyFactory, serviceProxyFactory: serviceProxyFactory)
                .WithMediator()
                .WithPools();

            _telemetryClient = _kernel.Get<TelemetryClient>();
            _mediator = _kernel.Get<Mediator>();
        }
        public async Task StartAsync(StartPoolRequest request)
        {
            await _mediator.ExecuteAsync(
                new StartPool(
                    this.GetActorId().GetStringId(), 
                    request.IsServiceStateful, 
                    request.HasPersistedState, 
                    request.MinReplicas, 
                    request.TargetReplicas,
                    (PartitionSchemeDescription)Enum.Parse(typeof(PartitionSchemeDescription), request.PartitionScheme.ToString()), 
                    request.MaxPoolSize, 
                    request.IdleServicesPoolSize, 
                    request.ServicesAllocationBlockSize, 
                    request.ExpirationQuanta), 
                default(CancellationToken));
            await SetReminderAsync(EnsurePoolSizeReminderKey, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        }   

        public async Task<ConfigurationResponse> GetConfigurationAsync()
        {
            var config = await _mediator.ExecuteAsync(new GetPoolConfiguration(), default(CancellationToken));
            return new ConfigurationResponse(
                config.ExpirationQuanta, 
                config.HasPersistedState,
                config.IdleServicesPoolSize, 
                config.IsServiceStateful, 
                config.MaxPoolSize,
                config.MinReplicaSetSize, 
                (SDK.PartitionSchemeDescription)Enum.Parse(typeof(SDK.PartitionSchemeDescription), config.PartitionScheme.ToString()), 
                config.ServicesAllocationBlockSize,
                config.ServiceTypeUri, 
                config.TargetReplicasetSize);
        }

        public async Task<PopVacantInstanceResponse> PopVacantInstanceAsync(PopVacantInstanceRequest request)
        {
            var result = await _mediator.ExecuteAsync(new PopVacantInstance(), default(CancellationToken));
            return new PopVacantInstanceResponse(result.InstanceId);
        }
               
        public async Task<GetVacantInstancesResponse> GetVacantInstancesAsync()
        {
            var result = await _mediator.ExecuteAsync(new GetVacantInstances(), default(CancellationToken));
            return new GetVacantInstancesResponse(result.VacantInstances);
        }

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
                            await _mediator.ExecuteAsync(new EnsurePoolSize(), default(CancellationToken));
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