using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances
{
    [Serializable]
    [DataContract]
    public class ServiceConfiguration
    {
        public ServiceConfiguration(Uri serviceInstanceUri, string poolId, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, SDK.PartitionSchemeDescription partitionScheme, TimeSpan expirationQuanta)
        {
            ServiceInstanceUri = serviceInstanceUri;
            PoolId = poolId;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            ExpirationQuanta = expirationQuanta;
        }

        [DataMember]
        public Uri ServiceInstanceUri { get; private set; }

        [DataMember]
        public string PoolId { get; private set; }

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
        public TimeSpan ExpirationQuanta { get; private set; }
    }
}
