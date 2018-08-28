using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances.Interfaces
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