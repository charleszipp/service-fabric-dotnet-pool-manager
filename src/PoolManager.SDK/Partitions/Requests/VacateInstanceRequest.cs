using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Requests
{
    [Serializable]
    [DataContract]
    public class VacateInstanceRequest
    {
        public VacateInstanceRequest(Guid instanceId)
        {
            InstanceId = instanceId;
        }

        [DataMember]
        public Guid InstanceId { get; private set; }
    }
}