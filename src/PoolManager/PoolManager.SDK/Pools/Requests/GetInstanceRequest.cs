using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Requests
{
    [Serializable]
    [DataContract]
    public class GetInstanceRequest
    {
        public GetInstanceRequest(string serviceInstanceName)
        {
            ServiceInstanceName = serviceInstanceName;
        }

        [DataMember]
        public string ServiceInstanceName { get; private set; }
    }
}