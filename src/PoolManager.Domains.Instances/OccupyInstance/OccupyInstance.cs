using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances
{
    public class OccupyInstance : ICommand
    {
        public OccupyInstance(Guid instanceId, string partitionId, string instanceName)
        {
            InstanceId = instanceId;
            PartitionId = partitionId;
            InstanceName = instanceName;
        }

        public Guid InstanceId { get; }

        public string PartitionId { get; }

        public string InstanceName { get; }
    }
}