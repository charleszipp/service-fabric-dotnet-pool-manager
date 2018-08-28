using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances.Interfaces
{
    [Serializable]
    [DataContract]
    public class StartRequest
    {
        public StartRequest(string serviceTypeUri, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, PartitionSchemeDescription partitionScheme)
        {
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public bool IsServiceStateful { get; private set; }
        [DataMember]
        public bool HasPersistedState { get; private set; }
        [DataMember]
        public int MinReplicas { get; private set; }
        [DataMember]
        public int TargetReplicas { get; private set; }
        [DataMember]
        public PartitionSchemeDescription PartitionScheme { get; private set; }
    }
}
