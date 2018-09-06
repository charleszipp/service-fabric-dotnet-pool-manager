using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.Core;
using PoolManager.SDK;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools;
using System;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class InstanceContext
    {
        private const string InstanceStateKey = "instance-state";
        private InstanceState _currentState;

        public InstanceContext(string instanceId, IInstanceStateProvider instanceStates, IPoolProxy poolProxy, IServiceProxyFactory proxyFactory, IClusterClient cluster, IActorStateManager stateManager, TelemetryClient telemetryClient)
        {
            InstanceStates = instanceStates;
            PoolProxy = poolProxy;
            InstanceId = instanceId;
            ProxyFactory = proxyFactory;
            Cluster = cluster;
            StateManager = stateManager;
            TelemetryClient = telemetryClient;
            _currentState = InstanceStates.Get(SDK.Instances.InstanceStates.Idle);
        }

        public IActorStateManager StateManager { get; }

        public string InstanceId { get; }

        public IServiceProxyFactory ProxyFactory { get; }

        public IClusterClient Cluster { get; }

        public TelemetryClient TelemetryClient { get; }

        public IInstanceStateProvider InstanceStates { get; }

        public IPoolProxy PoolProxy { get; }

        public async Task ActivateAsync()
        {
            var state = await StateManager.TryGetStateAsync<InstanceStates>(InstanceStateKey);
            if (state.HasValue)
                _currentState = InstanceStates.Get(state.Value);
        }

        public Task DeactivateAsync() => 
            SaveInstanceStateAsync();

        public async Task StartAsync(StartInstanceRequest request)
        {
            _currentState = await _currentState.StartAsync(this, request);
            await SaveInstanceStateAsync();
        }

        public async Task StartAsAsync(StartInstanceAsRequest request)
        {
            _currentState = await _currentState.StartAsAsync(this, request);
            await SaveInstanceStateAsync();
        }

        public async Task RemoveAsync()
        {
            _currentState = await _currentState.RemoveAsync(this);
            await SaveInstanceStateAsync();
        }

        public async Task VacateAsync()
        {
            _currentState = await _currentState.VacateAsync(this);
            await SaveInstanceStateAsync();
        }

        public async Task OccupyAsync(OccupyRequest request)
        {
            _currentState = await _currentState.OccupyAsync(this, request);
            await SaveInstanceStateAsync();
        }

        public Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request) => _currentState.ReportActivityAsync(this, request);

        internal Task SetInstanceConfigurationAsync(ServiceConfiguration instanceConfiguration) => StateManager.GetOrAddStateAsync("instance-configuration", instanceConfiguration);

        internal Task<ServiceConfiguration> GetInstanceConfigurationAsync() => StateManager.GetStateAsync<ServiceConfiguration>("instance-configuration");

        internal Task<ServiceState> GetServiceStateAsync() => StateManager.GetStateAsync<ServiceState>("service-state");

        internal Task SetServiceStateAsync(ServiceState serviceState) => StateManager.SetStateAsync("service-state", serviceState);

        internal async Task<IServiceInstance> GetServiceInstanceProxy()
        {
            ServiceConfiguration config = await GetInstanceConfigurationAsync();
            ServicePartitionKey servicePartitionKey = config.PartitionScheme.ToServicePartitionKey(InstanceId);
            var instanceProxy = ProxyFactory.CreateServiceProxy<IServiceInstance>(config.ServiceInstanceUri, servicePartitionKey);
            return instanceProxy;
        }

        private Task SaveInstanceStateAsync() =>
            StateManager.SetStateAsync(InstanceStateKey, _currentState.State);
    }
}