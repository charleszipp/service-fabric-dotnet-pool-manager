using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.SDK;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class InstanceContext
    {
        private const string _instanceStateKey = "instance-state";
        private InstanceState _currentState;

        public InstanceContext(string instanceId, IInstanceStateProvider instanceStates, IPoolProxy poolProxy, IServiceProxyFactory proxyFactory, FabricClient fabricClient, IActorStateManager stateManager, TelemetryClient telemetryClient)
        {
            InstanceStates = instanceStates;
            PoolProxy = poolProxy;
            InstanceId = instanceId;
            ProxyFactory = proxyFactory;
            FabricClient = fabricClient;
            StateManager = stateManager;
            TelemetryClient = telemetryClient;
            _currentState = InstanceStates.Get(SDK.Instances.InstanceStates.Idle);
        }

        public IActorStateManager StateManager { get; private set; }

        public string InstanceId { get; }

        public IServiceProxyFactory ProxyFactory { get; }

        public FabricClient FabricClient { get; }

        public TelemetryClient TelemetryClient { get; }

        public IInstanceStateProvider InstanceStates { get; }

        public IPoolProxy PoolProxy { get; }

        public async Task ActivateAsync()
        {
            var state = await StateManager.TryGetStateAsync<InstanceStates>(_instanceStateKey);
            if (state.HasValue)
                _currentState = InstanceStates.Get(state.Value);
        }

        public async Task DeactivateAsync()
        {
            await StateManager.SetStateAsync(_instanceStateKey, _currentState.State);
        }

        public async Task StartAsync(StartInstanceRequest request) => _currentState = await _currentState.StartAsync(this, request);

        public async Task StartAsAsync(StartInstanceAsRequest request) => _currentState = await _currentState.StartAsAsync(this, request);

        public async Task RemoveAsync() => _currentState = await _currentState.RemoveAsync(this);

        public async Task VacateAsync() => _currentState = await _currentState.VacateAsync(this);

        public async Task OccupyAsync(OccupyRequest request) => _currentState = await _currentState.OccupyAsync(this, request);

        public Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request) => _currentState.ReportActivityAsync(this, request);

        internal void ParseServiceTypeUri(string serviceTypeUri, out string applicationName, out string serviceTypeName)
        {
            var indexLastSlash = serviceTypeUri.LastIndexOf('/');
            if (indexLastSlash >= 0)
            {
                applicationName = serviceTypeUri.Substring(0, indexLastSlash);
                serviceTypeName = serviceTypeUri.Substring(indexLastSlash + 1);
            }
            else
            {
                applicationName = null;
                serviceTypeName = null;
            }
        }

        internal Uri CreateServiceInstanceUri(string serviceTypeUri, string serviceInstanceName)
        {
            return new Uri($"{serviceTypeUri}/{serviceInstanceName}", UriKind.RelativeOrAbsolute);
        }

        internal Task SetInstanceConfigurationAsync(ServiceConfiguration instanceConfiguration) => StateManager.GetOrAddStateAsync("instance-configuration", instanceConfiguration);

        internal Task<ServiceConfiguration> GetInstanceConfigurationAsync() => StateManager.GetStateAsync<ServiceConfiguration>("instance-configuration");

        internal Task<ServiceState> GetServiceStateAsync() => StateManager.GetStateAsync<ServiceState>("service-state");

        internal Task SetServiceStateAsync(ServiceState serviceState) => StateManager.SetStateAsync("service-state", serviceState);

        internal async Task<IServiceInstance> GetServiceInstanceProxy()
        {
            ServiceConfiguration config = await GetInstanceConfigurationAsync();
            ServicePartitionKey servicePartitionKey = null;

            switch (config.PartitionScheme)
            {
                case SDK.PartitionSchemeDescription.Singleton:
                    servicePartitionKey = null;
                    break;

                case SDK.PartitionSchemeDescription.UniformInt64Name:
                    servicePartitionKey = new ServicePartitionKey(1);
                    break;

                case SDK.PartitionSchemeDescription.Named:
                    servicePartitionKey = new ServicePartitionKey(InstanceId);
                    break;
            }

            var instanceProxy = ProxyFactory.CreateServiceProxy<IServiceInstance>(config.ServiceInstanceUri, servicePartitionKey);
            return instanceProxy;
        }
    }
}