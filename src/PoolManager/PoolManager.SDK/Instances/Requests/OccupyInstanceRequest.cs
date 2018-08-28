using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class OccupyRequest
    {
        public OccupyRequest(string serviceInstanceName, TimeSpan expirationQuanta)
        {
            ServiceInstanceName = serviceInstanceName;
            ExpirationQuanta = expirationQuanta;
        }

        [DataMember]
        public string ServiceInstanceName { get; private set; }

        [DataMember]
        public TimeSpan ExpirationQuanta { get; private set; }
    }
}