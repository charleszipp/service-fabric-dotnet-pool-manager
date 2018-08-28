using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances
{
    [Serializable]
    [DataContract]
    public class ServiceConfiguration
    {
        public ServiceConfiguration(Uri serviceInstanceUri, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, SDK.PartitionSchemeDescription partitionScheme)
        {
            ServiceInstanceUri = serviceInstanceUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
        }

        [DataMember]
        public Uri ServiceInstanceUri { get; private set; }

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
    }
}
