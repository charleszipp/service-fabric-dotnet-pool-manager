﻿using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class StartInstanceAsRequest
    {
        public StartInstanceAsRequest(
            string serviceInstanceName,
            string serviceTypeUri,
            bool isServiceStateful = true,
            bool hasPersistedState = true,
            int minReplicas = 1,
            int targetReplicas = 3,
            PartitionSchemeDescription partitionScheme = PartitionSchemeDescription.UniformInt64Name,
            TimeSpan? expirationQuanta = null
            )
        {
            ServiceInstanceName = serviceInstanceName;
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            PartitionScheme = partitionScheme;
            ExpirationQuanta = expirationQuanta ?? new TimeSpan(24, 0, 0);
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
        public TimeSpan ExpirationQuanta { get; private set; }
    }
}
