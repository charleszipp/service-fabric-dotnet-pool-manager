using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using System;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class PartitionProxy : IPartitionProxy
    {
        private readonly SDK.Partitions.IPartitionProxy partitions;

        public PartitionProxy(SDK.Partitions.IPartitionProxy partitions) => 
            this.partitions = partitions;

        public Task VacateInstanceAsync(string partitionId, string serviceTypeUri, string instanceName, Guid instanceId) =>
            partitions.VacateInstanceAsync(partitionId, new SDK.Partitions.Requests.VacateInstanceRequest(serviceTypeUri, instanceName, instanceId));
    }
}