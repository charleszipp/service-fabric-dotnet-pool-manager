using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Requests
{
    [Serializable]
    [DataContract]
    public class VacateInstanceRequest
    {
        public VacateInstanceRequest(Guid instanceId, string serviceInstanceName)
        {
            InstanceId = instanceId;
            ServiceInstanceName = serviceInstanceName;
        }

        [DataMember]
        public Guid InstanceId { get; private set; }

        [DataMember]
        public string ServiceInstanceName { get; private set; }
    }
}