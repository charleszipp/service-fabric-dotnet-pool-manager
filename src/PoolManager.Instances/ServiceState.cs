using PoolManager.SDK.Instances;
using System;
using System.Runtime.Serialization;

namespace PoolManager.Instances
{
    [Serializable]
    [DataContract]
    public class ServiceState
    {
        public ServiceState()
        {
            State = ServiceStates.Vacant;
        }

        public ServiceState(string serviceInstanceName)
        {
            State = ServiceStates.Occupied;
            ServiceInstanceName = serviceInstanceName;
            LastActiveUtc = DateTime.UtcNow;
        }

        [DataMember]
        public ServiceStates State { get; private set; }

        [DataMember]
        public string ServiceInstanceName { get; private set; }

        [DataMember]
        public DateTime LastActiveUtc { get; set; }
    }
}