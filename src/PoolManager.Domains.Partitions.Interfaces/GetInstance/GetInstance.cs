using PoolManager.Core.Mediators.Commands;

namespace PoolManager.Domains.Partitions.Interfaces
{
    public class GetInstance : ICommand<GetInstanceResult>
    {
        public GetInstance(string serviceTypeUri, string instanceName, string partitionId)
        {
            ServiceTypeUri = serviceTypeUri;
            InstanceName = instanceName;
            PartitionId = partitionId;
        }

        public string InstanceName { get; }

        public string PartitionId { get; }

        public string ServiceTypeUri { get; }
    }
}