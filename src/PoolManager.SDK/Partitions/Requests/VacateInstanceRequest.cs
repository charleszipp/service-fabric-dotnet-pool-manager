using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Requests
{
    [Serializable]
    [DataContract]
    public class VacateInstanceRequest
    {
        public VacateInstanceRequest(string serviceTypeUri, string instanceName)
        {
            ServiceTypeUri = serviceTypeUri;
            InstanceName = instanceName;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public string InstanceName { get; private set; }
    }
}