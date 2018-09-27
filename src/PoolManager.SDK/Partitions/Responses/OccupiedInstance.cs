using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Responses
{
    [DataContract]
    public class OccupiedInstance
    {
        public OccupiedInstance(Guid id, Uri serviceName, string instanceName, string partitionId)
        {
            Id = id;
            ServiceName = serviceName;
            InstanceName = instanceName;
            PartitionId = partitionId;
        }

        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public Uri ServiceName { get; private set; }
        [DataMember]
        public string InstanceName { get; private set; }
        [DataMember]
        public string PartitionId { get; private set; }
    }
}