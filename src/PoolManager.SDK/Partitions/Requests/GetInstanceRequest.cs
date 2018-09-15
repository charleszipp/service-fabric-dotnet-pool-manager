using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Requests
{
    [Serializable]
    [DataContract]
    public class GetInstanceRequest
    {
        public GetInstanceRequest(string serviceTypeUri, string serviceInstanceName)
        {
            ServiceTypeUri = serviceTypeUri;
            ServiceInstanceName = serviceInstanceName;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public string ServiceInstanceName { get; private set; }
    }
}