using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Responses
{
    [Serializable]
    [DataContract]
    public class ConfigurationResponse
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
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public int TargetReplicasetSize { get; private set; }
        public ConfigurationResponse(TimeSpan expirationQuanta, bool hasPersistedState, int idleServicesPoolSize, bool isServiceStateful,
            int maxPoolSize, int minReplicaSetSize, PartitionSchemeDescription partitionScheme, int servicesAllocationBlockSize,
            string serviceTypeUri, int targetReplicasetSize)
        {
            ExpirationQuanta = expirationQuanta;
            HasPersistedState = hasPersistedState;
            IdleServicesPoolSize = idleServicesPoolSize;
            IsServiceStateful = isServiceStateful;
            MaxPoolSize = maxPoolSize;
            MinReplicaSetSize = minReplicaSetSize;
            PartitionScheme = partitionScheme;
            ServicesAllocationBlockSize = servicesAllocationBlockSize;
            ServiceTypeUri = serviceTypeUri;
            TargetReplicasetSize = targetReplicasetSize;
        }
    }
}