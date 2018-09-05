using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Instances.Requests
{
    [Serializable]
    [DataContract]
    public class ReportActivityRequest
    {
        public ReportActivityRequest(DateTime lastActiveUtc)
        {
            LastActiveUtc = lastActiveUtc;
        }

        [DataMember]
        public DateTime LastActiveUtc { get; private set; }
    }
}