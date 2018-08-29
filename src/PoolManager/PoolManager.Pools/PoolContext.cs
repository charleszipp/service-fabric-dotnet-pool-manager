using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolContext
    {
        private PoolState _currentState;
        private const string _poolStateKey = "pool-state";

        public PoolContext(string serviceTypeUri, IPoolStateProvider poolStates, IInstanceProxy instanceProxy, IActorStateManager stateManager, TelemetryClient telemetryClient)
        {
            ServiceTypeUri = serviceTypeUri;
            PoolStates = poolStates;
            InstanceProxy = instanceProxy;
            StateManager = stateManager;
            TelemetryClient = telemetryClient;
            _currentState = PoolStates.Get(SDK.Pools.PoolStates.Idle);
        }

        public string ServiceTypeUri { get; }
        public IPoolStateProvider PoolStates { get; }
        public IInstanceProxy InstanceProxy { get; }
        public IServiceProxyFactory ServiceProxyFactory { get; }
        public IActorStateManager StateManager { get; }
        public TelemetryClient TelemetryClient { get; }

        public async Task ActivateAsync()
        {
            var state = await StateManager.TryGetStateAsync<PoolStates>(_poolStateKey);
            if (state.HasValue)
                _currentState = PoolStates.Get(state.Value);
        }

        public Task DeactivateAsync() => StateManager.SetStateAsync(_poolStateKey, _currentState.State);

        public async Task StartAsync(StartPoolRequest request) => _currentState = await _currentState.StartAsync(this, request);

        public Task GetAsync(GetInstanceRequest request) => _currentState.GetAsync(this, request);

        public Task VacateInstanceAsync(VacateInstanceRequest request) => _currentState.VacateInstanceAsync(this, request);

        internal Task<PoolInstances> GetPoolInstancesAsync() => StateManager.GetOrAddStateAsync("pool-instances", new PoolInstances());

        internal Task<PoolConfiguration> GetPoolConfigurationAsync() => StateManager.GetStateAsync<PoolConfiguration>("pool-configuration");

        internal Task SetPoolInstancesAsync(PoolInstances poolInstances) => StateManager.SetStateAsync("pool-instances", poolInstances);

        internal Task SetPoolConfigurationAsync(PoolConfiguration poolConfiguration) => StateManager.SetStateAsync("pool-configuration", poolConfiguration);

        internal async Task AddInstanceAsAsync(string serviceInstanceName, PoolConfiguration configuration, PoolInstances poolInstances)
        {            
            var instanceId = await InstanceProxy.StartAsAsync(new SDK.Instances.Requests.StartInstanceAsRequest(
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

        private async Task RemoveInstanceAsync(ConcurrentQueue<Guid> vacantInstances)
        {
            Guid instanceId = Guid.Empty;
            if(vacantInstances.TryDequeue(out instanceId))
                await InstanceProxy.RemoveAsync(instanceId);
        }

        public async Task EnsurePoolSizeAsync(PoolConfiguration configuration = null)
        {
            configuration = configuration ?? await GetPoolConfigurationAsync();
            var poolInstances = await GetPoolInstancesAsync();
            var activeInstances = poolInstances.OccupiedInstances;
            var idleInstances = poolInstances.VacantInstances;

            long idleInstancesCount = 0;
            long activeInstancesCount = 0;

            idleInstancesCount = idleInstances.Count;
            activeInstancesCount = activeInstances.Count;

            long allInstancesCount = idleInstancesCount + activeInstancesCount;
            long idleInstanceDelta = configuration.IdleServicesPoolSize - idleInstancesCount;

            long allocationCount = 0;
            if (idleInstanceDelta > 0)
            {
                allocationCount = Math.Min(configuration.MaxPoolSize - allInstancesCount, idleInstanceDelta);
            }
            else if (idleInstanceDelta < 0)
            {
                allocationCount = -idleInstanceDelta;
            }
            else
                return;

            while (allocationCount > 0)
            {
                if (idleInstanceDelta > 0)
                {
                    List<Task> addTasks = new List<Task>();
                    for (int i = 0; i < configuration.ServicesAllocationBlockSize && i < allocationCount; i++)
                        addTasks.Add(AddInstanceAsync(configuration, poolInstances));
                    Task.WaitAll(addTasks.ToArray());
                }
                else
                {
                    List<Task> removeTasks = new List<Task>();
                    for (int i = 0; i < configuration.ServicesAllocationBlockSize && i < allocationCount; i++)
                        removeTasks.Add(RemoveInstanceAsync(poolInstances.VacantInstances));
                    Task.WaitAll(removeTasks.ToArray());
                }
                allocationCount -= configuration.ServicesAllocationBlockSize;
            }

            await SetPoolInstancesAsync(poolInstances);
        }
    }
}