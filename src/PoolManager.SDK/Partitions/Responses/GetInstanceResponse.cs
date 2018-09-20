using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Responses
{
    [DataContract]
    public class GetInstanceResponse
    {
        public GetInstanceResponse(Uri serviceInstanceUri)
        {
            ServiceInstanceUri = serviceInstanceUri;
        }

        [DataMember]
        public Uri ServiceInstanceUri { get; private set; }
    }
}