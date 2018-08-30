using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances.Interfaces
{
    [Serializable]
    [DataContract]
    public class StartAsRequest
    {
        public StartAsRequest(string serviceInstanceName, string serviceTypeUri, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, PartitionSchemeDescription partitionScheme, TimeSpan expirationQuanta)
        {
            ServiceInstanceName = serviceInstanceName;
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            ExpirationQuanta = expirationQuanta;
        }

        [DataMember]
        public string ServiceInstanceName { get; private set; }
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
        [DataMember]
        public TimeSpan ExpirationQuanta { get; }
    }
}
