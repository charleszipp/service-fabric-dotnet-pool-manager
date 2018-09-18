using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class OccupyRequest
    {
        public OccupyRequest(string partitionId, string serviceInstanceName)
        {
            PartitionId = partitionId;
            ServiceInstanceName = serviceInstanceName;
        }

        [DataMember]
        public string PartitionId { get; private set; }
        [DataMember]
        public string ServiceInstanceName { get; private set; }
    }
}