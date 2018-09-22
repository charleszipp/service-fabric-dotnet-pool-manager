using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Responses
{
    [Serializable]
    [DataContract]
    public class PopVacantInstanceResponse
    {
        public PopVacantInstanceResponse(Guid? instanceId)
        {
            InstanceId = instanceId;
        }

        [DataMember]
        public Guid? InstanceId { get; private set; }
    }
}