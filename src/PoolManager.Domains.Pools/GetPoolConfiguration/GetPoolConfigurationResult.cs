using System;

namespace PoolManager.Domains.Pools
{
    public class GetPoolConfigurationResult
    {
        public GetPoolConfigurationResult(
            TimeSpan expirationQuanta, 
            bool hasPersistedState, 
            int idleServicesPoolSize, 
            bool isServiceStateful,
            int maxPoolSize, 
            int minReplicaSetSize, 
            PartitionSchemeDescription partitionScheme, 
            int servicesAllocationBlockSize,
            string serviceTypeUri, 
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
            ServiceTypeUri = serviceTypeUri;
            TargetReplicasetSize = targetReplicasetSize;
        }

        public TimeSpan ExpirationQuanta { get; }

        public bool HasPersistedState { get; }

        public int IdleServicesPoolSize { get; }

        public bool IsServiceStateful { get; }

        public int MaxPoolSize { get; }

        public int MinReplicaSetSize { get; }

        public PartitionSchemeDescription PartitionScheme { get; }

        public int ServicesAllocationBlockSize { get; }

        public string ServiceTypeUri { get; }

        public int TargetReplicasetSize { get; }
    }
}