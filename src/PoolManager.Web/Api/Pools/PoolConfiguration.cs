using System;
using System.Runtime.Serialization;

namespace PoolManager.Web.Api.Pools
{
    [DataContract]
    public class PoolConfiguration
    {
        [DataMember]
        public TimeSpan ExpirationQuanta { get; private set; }
        [DataMember]
        public bool HasPersistedState { get; private set; }
        [DataMember]
        public int IdleServicesPoolSize { get; private set; }
        [DataMember]
        public bool IsServiceStateful { get; private set; }
        [DataMember]
        public int MaxPoolSize { get; private set; }
        [DataMember]
        public int MinReplicaSetSize { get; private set; }
        [DataMember]
        public PartitionSchemeDescription PartitionScheme { get; private set; }
        [DataMember]
        public int ServicesAllocationBlockSize { get; private set; }
        [DataMember]
        public int TargetReplicasetSize { get; private set; }
        public PoolConfiguration(TimeSpan expirationQuanta, bool hasPersistedState, int idleServicesPoolSize, bool isServiceStateful,
            int maxPoolSize, int minReplicaSetSize, PartitionSchemeDescription partitionScheme, int servicesAllocationBlockSize, 
            int targetReplicasetSize)
        {
            ExpirationQuanta = expirationQuanta;
            HasPersistedState = hasPersistedState;
            IdleServicesPoolSize = idleServicesPoolSize;
            IsServiceStateful = isServiceStateful;
            MaxPoolSize = maxPoolSize;
            MinReplicaSetSize = minReplicaSetSize;
            PartitionScheme = partitionScheme;
            ServicesAllocationBlockSize = servicesAllocationBlockSize;
            TargetReplicasetSize = targetReplicasetSize;
        }
    }
}
