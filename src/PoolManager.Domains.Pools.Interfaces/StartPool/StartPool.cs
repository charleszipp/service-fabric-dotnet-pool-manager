using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Pools.Interfaces
{
    public class StartPool : ICommand
    {
        public StartPool(
            string serviceTypeUri,
            bool isServiceStateful,
            bool hasPersistedState,
            int minReplicas,
            int targetReplicas,
            PartitionSchemeDescription partitionScheme,
            int maxPoolSize,
            int idleServicesPoolSize,
            int servicesAllocationBlockSize,
            TimeSpan expirationQuanta
            )
        {
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            MaxPoolSize = maxPoolSize;
            IdleServicesPoolSize = idleServicesPoolSize;
            ServicesAllocationBlockSize = servicesAllocationBlockSize;
            ExpirationQuanta = expirationQuanta;
        }

        public string ServiceTypeUri { get; }

        public bool IsServiceStateful { get; }

        public bool HasPersistedState { get; }

        public int MinReplicas { get; }

        public int TargetReplicas { get; }

        public PartitionSchemeDescription PartitionScheme { get; }

        public int MaxPoolSize { get; }

        public int IdleServicesPoolSize { get; }

        public int ServicesAllocationBlockSize { get; }

        public TimeSpan ExpirationQuanta { get; }
    }
}