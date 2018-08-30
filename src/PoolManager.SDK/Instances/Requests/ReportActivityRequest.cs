using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class ReportActivityRequest
    {
        public ReportActivityRequest(string serviceInstanceName, DateTime lastActiveUtc)
        {
            ServiceInstanceName = serviceInstanceName;
            LastActiveUtc = lastActiveUtc;
        }
        [DataMember]
        public string ServiceInstanceName { get; private set; }

        [DataMember]
        public DateTime LastActiveUtc { get; private set; }
    }
}