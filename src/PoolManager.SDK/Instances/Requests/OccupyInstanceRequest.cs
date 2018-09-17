using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class OccupyRequest
    {
        public OccupyRequest(string serviceInstanceName)
        {
            ServiceInstanceName = serviceInstanceName;
        }

        [DataMember]
        public string ServiceInstanceName { get; private set; }
    }
}