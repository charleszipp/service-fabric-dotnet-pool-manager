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

        public Task DeactivateAsync() => StateManager.SetStateAsync(PoolStateKey, _currentState.State);

        public async Task StartAsync(StartPoolRequest request)
        {
            _currentState = await _currentState.StartAsync(this, request);
            await StateManager.SetStateAsync(PoolStateKey, _currentState.State);
        }

        public async Task StopAsync()
        {
            _currentState = await _currentState.StopAsync(this);
            await StateManager.SetStateAsync(PoolStateKey, _currentState.State);
        }

        public Task<GetInstanceResponse> GetAsync(GetInstanceRequest request) => _currentState.GetAsync(this, request);

        public Task VacateInstanceAsync(VacateInstanceRequest request) => _currentState.VacateInstanceAsync(this, request);

        internal Task<PoolInstances> GetPoolInstancesAsync() => StateManager.GetOrAddStateAsync("pool-instances", new PoolInstances());

        internal Task<PoolConfiguration> GetPoolConfigurationAsync() => StateManager.GetStateAsync<PoolConfiguration>("pool-configuration");

        internal Task SetPoolInstancesAsync(PoolInstances poolInstances) => StateManager.SetStateAsync("pool-instances", poolInstances);

        internal Task SetPoolConfigurationAsync(PoolConfiguration poolConfiguration) => StateManager.SetStateAsync("pool-configuration", poolConfiguration);

        internal async Task AddInstanceAsAsync(string serviceInstanceName, PoolConfiguration configuration, PoolInstances poolInstances)
        {            
            var instanceId = await InstanceProxy.StartAsAsync(new SDK.Instances.Requests.StartInstanceAsRequest(
                PoolId,
                serviceInstanceName,
                ServiceTypeUri,
                configuration.IsServiceStateful,
                configuration.HasPersistedState,
                configuration.MinReplicaSetSize,
                configuration.TargetReplicasetSize,
                configuration.PartitionScheme,
                configuration.ExpirationQuanta
                )
            );

            poolInstances.OccupiedInstances[serviceInstanceName] = instanceId;
        }

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
        private async Task RemoveInstanceAsync(ConcurrentQueue<Guid> vacantInstances)
        {
            if (vacantInstances.TryDequeue(out var instanceId)) await InstanceProxy.RemoveAsync(instanceId);
        }

        public async Task EnsurePoolSizeAsync(PoolConfiguration configuration = null)
        {
            configuration = configuration ?? await GetPoolConfigurationAsync();
            var poolInstances = await GetPoolInstancesAsync();
            var activeInstances = poolInstances.OccupiedInstances;
            var idleInstances = poolInstances.VacantInstances;

            long idleInstancesCount = idleInstances.Count;
            long activeInstancesCount = activeInstances.Count;

            var allInstancesCount = idleInstancesCount + activeInstancesCount;
            var idleInstanceDelta = configuration.IdleServicesPoolSize - idleInstancesCount;

            TelemetryClient.GetMetric("pools.occupied.size", nameof(ServiceTypeUri)).TrackValue(activeInstancesCount, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.size", nameof(ServiceTypeUri)).TrackValue(idleInstancesCount, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.target", nameof(ServiceTypeUri)).TrackValue(configuration.IdleServicesPoolSize, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.max", nameof(ServiceTypeUri)).TrackValue(configuration.MaxPoolSize, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.block.size", nameof(ServiceTypeUri)).TrackValue(configuration.IdleServicesPoolSize, ServiceTypeUri);
            TelemetryClient.GetMetric("pools.vacant.deficit", nameof(ServiceTypeUri)).TrackValue(idleInstanceDelta, ServiceTypeUri);

            long allocationCount;
            if (idleInstanceDelta > 0)
            {
                allocationCount = Math.Min(configuration.MaxPoolSize - allInstancesCount, idleInstanceDelta);
                TelemetryClient.GetMetric("pools.vacant.grow", nameof(ServiceTypeUri)).TrackValue(allocationCount, ServiceTypeUri);
            }
            else if (idleInstanceDelta < 0)
            {
                allocationCount = -idleInstanceDelta;
                TelemetryClient.GetMetric("pools.vacant.shrink", nameof(ServiceTypeUri)).TrackValue(allocationCount, ServiceTypeUri);
            }
            else
                return;


            while (allocationCount > 0)
            {
                if (idleInstanceDelta > 0)
                {
                    using (TelemetryClient.TrackMetricTimer("pools.vacant.grow.block.time", nameof(ServiceTypeUri), ServiceTypeUri))
                    {
                        var addTasks = new List<Task>();
                        for (var i = 0; i < configuration.ServicesAllocationBlockSize && i < allocationCount; i++)
                            addTasks.Add(AddInstanceAsync(configuration, poolInstances));
                        Task.WaitAll(addTasks.ToArray());
                    }
                }
                else
                {
                    using (TelemetryClient.TrackMetricTimer("pools.vacant.shrink.block.time", nameof(ServiceTypeUri), ServiceTypeUri))
                    {
                        var removeTasks = new List<Task>();
                        for (var i = 0; i < configuration.ServicesAllocationBlockSize && i < allocationCount; i++)
                            removeTasks.Add(RemoveInstanceAsync(poolInstances.VacantInstances));
                        Task.WaitAll(removeTasks.ToArray());
                    }
                }
                allocationCount -= configuration.ServicesAllocationBlockSize;
            }

            await SetPoolInstancesAsync(poolInstances);
        }

        public async Task CleanupRemovedInstancesAsync()
        {
            var poolInstances = await GetPoolInstancesAsync();
            TelemetryClient.GetMetric("pools.removed.size", nameof(ServiceTypeUri)).TrackValue(poolInstances.RemovedInstances.Count, ServiceTypeUri);

            var deletes = new List<Task>();
            try
            {                
                while (!poolInstances.RemovedInstances.IsEmpty)
                {
                    if (poolInstances.RemovedInstances.TryDequeue(out var instanceId))
                        deletes.Add(InstanceProxy.DeleteAsync(instanceId));
                }
                await Task.WhenAll(deletes);
            }
            finally
            {
                TelemetryClient.GetMetric("pools.removed.completed", nameof(ServiceTypeUri)).TrackValue(deletes.Count(d => d.IsCompleted), ServiceTypeUri);
                TelemetryClient.GetMetric("pools.removed.failed", nameof(ServiceTypeUri)).TrackValue(deletes.Count(d => d.IsFaulted), ServiceTypeUri);
            }
        }

        private static string GetServiceTypeUri(string poolId) => 
            poolId.Contains("?") ? poolId.Substring(0, poolId.IndexOf('?')) : poolId;
    }
}