using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Domains.Partitions.Interfaces;
using PoolManager.Partitions.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Partitions
{
    public class PartitionRepository : IPartitionRepository
    {
        private readonly IActorStateManager stateManager;

        public PartitionRepository(IActorStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public Task SetOccupiedInstanceAsync(string serviceTypeUri, string instanceName, Guid instanceId, Uri serviceName) =>
            stateManager.SetStateAsync(GetStateName(serviceTypeUri, instanceName), new MappedInstance(instanceId, serviceName, instanceName));

        public async Task<Uri> TryGetOccupiedInstanceUriAsync(string serviceTypeUri, string instanceName, CancellationToken cancellationToken)
        {
            var instance = await stateManager.TryGetStateAsync<MappedInstance>(GetStateName(serviceTypeUri, instanceName), cancellationToken);
            return instance.Value?.ServiceName;
        }

        private string GetStateName(string serviceTypeUri, string serviceInstanceName) =>
            $"{serviceTypeUri}?{serviceInstanceName}";
    }
}