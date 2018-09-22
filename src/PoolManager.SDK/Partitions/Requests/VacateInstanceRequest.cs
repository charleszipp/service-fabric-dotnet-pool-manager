using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Requests
{
    [Serializable]
    [DataContract]
    public class VacateInstanceRequest
    {
        public VacateInstanceRequest(string serviceTypeUri, string instanceName, Guid instanceId)
        {
            ServiceTypeUri = serviceTypeUri;
            InstanceName = instanceName;
            InstanceId = instanceId;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public string InstanceName { get; private set; }
        [DataMember]
        public Guid InstanceId { get; private set; }
    }
}