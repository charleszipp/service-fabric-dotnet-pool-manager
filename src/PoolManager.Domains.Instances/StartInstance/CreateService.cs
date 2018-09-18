using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances
{
    public class CreateService : ICommand<CreateServiceResult>
    {
        public CreateService(
            Guid instanceId,
            string serviceTypeUri,
            bool isServiceStateful,
            bool hasPersistedState,
            int minReplicas,
            int targetReplicas,
            PartitionSchemeDescription partitionScheme
            )
        {
            InstanceId = instanceId;
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
        }

        public Guid InstanceId { get; }
        public string ServiceTypeUri { get; }

        public bool IsServiceStateful { get; }

        public bool HasPersistedState { get; }

        public int MinReplicas { get; }

        public int TargetReplicas { get; }

        public PartitionSchemeDescription PartitionScheme { get; }
    }
}