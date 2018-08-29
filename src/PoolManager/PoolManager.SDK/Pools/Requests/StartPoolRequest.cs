using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Requests
{
    [Serializable]
    [DataContract]
    public class StartPoolRequest
    {
        public StartPoolRequest(
            bool isServiceStateful = true,
            bool hasPersistedState = true,
            int minReplicas = 1,
            int targetReplicas = 3,
            PartitionSchemeDescription partitionScheme = PartitionSchemeDescription.UniformInt64Name,
            int maxPoolSize = Int32.MaxValue,
            int idleServicesPoolSize = 10,
            int servicesAllocationBlockSize = 5,
            TimeSpan? expirationQuanta = null
            )
        {
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            MaxPoolSize = maxPoolSize;
            IdleServicesPoolSize = idleServicesPoolSize;
            ServicesAllocationBlockSize = servicesAllocationBlockSize;
            ExpirationQuanta = expirationQuanta ?? new TimeSpan(24, 0, 0);
        }

        [DataMember]
        public bool IsServiceStateful { get; private set; }
        [DataMember]
        public bool HasPersistedState { get; private set; }
        [DataMember]
        public int MinReplicas { get; private set; }
        [DataMember]
        public int TargetReplicas { get; private set; }
        [DataMember]
        public SDK.PartitionSchemeDescription PartitionScheme { get; private set; }
        [DataMember]
        public int MaxPoolSize { get; private set; }
        [DataMember]
        public int IdleServicesPoolSize { get; private set; }
        [DataMember]
        public int ServicesAllocationBlockSize { get; private set; }
        [DataMember]
        public TimeSpan ExpirationQuanta { get; private set; }
    }
}
