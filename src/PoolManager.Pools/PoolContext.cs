using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolContext
    {
        private PoolState _currentState;
        private const string PoolStateKey = "pool-state";

        public PoolContext(string poolId, IPoolStateProvider poolStates, IInstanceProxy instanceProxy, IActorStateManager stateManager, TelemetryClient telemetryClient)
        {
            ServiceTypeUri = GetServiceTypeUri(poolId);
            PoolId = poolId;
            PoolStates = poolStates;
            InstanceProxy = instanceProxy;
            StateManager = stateManager;
            TelemetryClient = telemetryClient;
            _currentState = PoolStates.Get(SDK.Pools.PoolStates.Idle);
        }

        public string ServiceTypeUri { get; }
        public string PoolId { get; }
        public IPoolStateProvider PoolStates { get; }
        public IInstanceProxy InstanceProxy { get; }
        public IActorStateManager StateManager { get; }
        public TelemetryClient TelemetryClient { get; }
        public PoolStates CurrentState => _currentState.State;

        public async Task ActivateAsync()
        {
            var state = await StateManager.TryGetStateAsync<PoolStates>(PoolStateKey);
            if (state.HasValue)
                _currentState = PoolStates.Get(state.Value);
        }

        public Task DeactivateAsync() => 
            StateManager.SetStateAsync(PoolStateKey, _currentState.State);

        public async Task StartAsync(StartPoolRequest request)
        {
            _currentState = await _currentState.StartAsync(this, request);
            await StateManager.SetStateAsync(PoolStateKey, _currentState.State);
        }

        public async Task EnsurePoolSizeAsync(PoolConfiguration configuration = null)
        {
            configuration = configuration ?? await GetPoolConfigurationAsync();
            var poolInstances = await GetPoolInstancesAsync();
            long idleInstancesCount = poolInstances.VacantInstances.Count;

            var idleInstanceDelta = configuration.IdleServicesPoolSize - idleInstancesCount;

            TelemetryClient.GetMetric("pools.vacant.size", nameof(ServiceTypeUri)).TrackValue(idleInstancesCount, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.target", nameof(ServiceTypeUri)).TrackValue(configuration.IdleServicesPoolSize, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.block.size", nameof(ServiceTypeUri)).TrackValue(configuration.IdleServicesPoolSize, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.deficit", nameof(ServiceTypeUri)).TrackValue(idleInstanceDelta, ServiceTypeUri);

            if (idleInstanceDelta == 0)
                return;

            while (idleInstanceDelta > 0)
            {
                using (TelemetryClient.TrackMetricTimer("pools.vacant.grow.block.time", nameof(ServiceTypeUri), ServiceTypeUri))
                {
                    var addTasks = new List<Task>();
                    for (var i = 0; i < configuration.ServicesAllocationBlockSize && i < idleInstanceDelta; i++)
                    {
                        addTasks.Add(AddInstanceAsync(configuration, poolInstances));
                        idleInstanceDelta--;
                    }

                    Task.WaitAll(addTasks.ToArray());
                }
            }

            await SetPoolInstancesAsync(poolInstances);
        }

        internal Task<PoolInstances> GetPoolInstancesAsync() => StateManager.GetOrAddStateAsync("pool-instances", new PoolInstances());

        internal Task<PoolConfiguration> GetPoolConfigurationAsync() => StateManager.GetStateAsync<PoolConfiguration>("pool-configuration");

        internal Task SetPoolInstancesAsync(PoolInstances poolInstances) => StateManager.SetStateAsync("pool-instances", poolInstances);

        internal Task SetPoolConfigurationAsync(PoolConfiguration poolConfiguration) => StateManager.SetStateAsync("pool-configuration", poolConfiguration);

        internal async Task AddInstanceAsync(PoolConfiguration configuration, PoolInstances poolInstances)
        {
            var instanceId = await InstanceProxy.StartAsync(new SDK.Instances.Requests.StartInstanceRequest(
                PoolId,
                ServiceTypeUri,
                configuration.IsServiceStateful,
                configuration.HasPersistedState,
                configuration.MinReplicaSetSize,
                configuration.TargetReplicasetSize,
                configuration.PartitionScheme,
                configuration.ExpirationQuanta
                )
            );

            poolInstances.VacantInstances.Enqueue(instanceId);
        }

        internal Uri CreateServiceInstanceUri(Guid instanceId)
        {
            return new Uri($"{ServiceTypeUri}/{instanceId}", UriKind.RelativeOrAbsolute);
        }

        private static string GetServiceTypeUri(string poolId) => 
            poolId.Contains("?") ? poolId.Substring(0, poolId.IndexOf('?')) : poolId;
    }
}