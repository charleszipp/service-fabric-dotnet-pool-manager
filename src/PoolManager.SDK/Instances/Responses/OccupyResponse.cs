using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Responses
{
    [Serializable]
    [DataContract]
    public class OccupyResponse
    {
        public OccupyResponse(Uri serviceName)
        {
            ServiceName = serviceName;
        }

        [DataMember]
        public Uri ServiceName { get; private set; }
    }
}