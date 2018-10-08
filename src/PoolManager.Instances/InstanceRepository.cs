using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class InstanceRepository : IInstanceRepository
    {
        private readonly IActorStateManager stateManager;
        private class StateNames
        {
            public const string ExpirationQuanta = "expiration-quanta";
            public const string InstanceState = "instance-state";
            public const string PartitionId = "partition-id";
            public const string ServiceInstanceName = "instance-name";
            public const string ServiceLastActive = "last-active";
            public const string ServiceUri = "service-uri";
            public const string ServiceTypeUri = "service-type-uri";
        }

        public InstanceRepository(IActorStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public Task<TimeSpan> GetExpirationQuantaAsync(CancellationToken cancellationToken) => 
            stateManager.GetStateAsync<TimeSpan>(StateNames.ExpirationQuanta, cancellationToken);

        public async Task<InstanceStates?> TryGetInstanceStateAsync(CancellationToken cancellationToken)
        {
            var instanceState = await stateManager.TryGetStateAsync<InstanceStates>(StateNames.InstanceState, cancellationToken);
            if (instanceState.HasValue)
                return instanceState.Value;
            else
                return null;
        }

        public Task<string> GetPartitionIdAsync(CancellationToken cancellationToken) =>
            stateManager.GetStateAsync<string>(StateNames.PartitionId, cancellationToken);

        public async Task<string> TryGetServiceInstanceNameAsync(CancellationToken cancellationToken)
        {
            var instanceName = await stateManager.TryGetStateAsync<string>(StateNames.ServiceInstanceName, cancellationToken);
            if (instanceName.HasValue)
                return instanceName.Value;
            else
                return null;
        }

        public Task<DateTime> GetServiceLastActiveAsync(CancellationToken cancellationToken) => 
            stateManager.GetStateAsync<DateTime>(StateNames.ServiceLastActive, cancellationToken);

        public Task<Uri> GetServiceUriAsync(CancellationToken cancellationToken) =>
            stateManager.GetStateAsync<Uri>(StateNames.ServiceUri, cancellationToken);

        public Task SetExprirationQuantaAsync(TimeSpan expirationQuanta, CancellationToken cancellationToken) => 
            stateManager.SetStateAsync(StateNames.ExpirationQuanta, expirationQuanta, cancellationToken);

        public Task SetInstanceStateAsync(InstanceStates state, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.InstanceState, state, cancellationToken);

        public Task SetServiceInstanceName(string instanceName, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.ServiceInstanceName, instanceName, cancellationToken);

        public Task SetServiceLastActiveAsync(DateTime lastActiveUtc, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.ServiceLastActive, lastActiveUtc, cancellationToken);

        public Task SetServiceUriAsync(Uri serviceUri, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.ServiceUri, serviceUri, cancellationToken);

        public Task UnsetServiceInstanceName(CancellationToken cancellationToken) =>
            stateManager.TryRemoveStateAsync(StateNames.ServiceInstanceName, cancellationToken);

        public Task SetPartitionIdAsync(string partitionId, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.PartitionId, partitionId, cancellationToken);

        public Task SetServiceTypeUriAsync(string serviceTypeUri, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(StateNames.ServiceTypeUri, serviceTypeUri, cancellationToken);

        public Task<string> GetServiceTypeUriAsync(CancellationToken cancellationToken) =>
            stateManager.GetStateAsync<string>(StateNames.ServiceTypeUri, cancellationToken);
    }
}