using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances.Interfaces
{
    public class StartInstance : ICommand
    {
        public StartInstance(
            Guid instanceId,
            string serviceTypeUri,
            bool isServiceStateful,
            bool hasPersistedState,
            int minReplicas,
            int targetReplicas,
            PartitionSchemeDescription partitionScheme,
            TimeSpan expirationQuanta
            )
        {
            InstanceId = instanceId;
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            ExpirationQuanta = expirationQuanta;
        }

        public Guid InstanceId { get; }

        public string ServiceTypeUri { get; }

        public bool IsServiceStateful { get; }

        public bool HasPersistedState { get; }

        public int MinReplicas { get; }

        public int TargetReplicas { get; }

        public PartitionSchemeDescription PartitionScheme { get; }

        public TimeSpan ExpirationQuanta { get; }
    }
}