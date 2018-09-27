using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using PoolManager.SDK.Partitions.Responses;

namespace PoolManager.SDK.Pools.Responses
{
    [DataContract]
    public class GetInstancesResponse
    {
        public GetInstancesResponse(string serviceTypeUri, IEnumerable<Guid> vacantInstances, IEnumerable<OccupiedInstance> occupiedInstances)
        {
            ServiceTypeUri = serviceTypeUri;
            VacantInstances = vacantInstances;
            OccupiedInstances = occupiedInstances;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
        [DataMember]
        public IEnumerable<Guid> VacantInstances { get; private set; }
        [DataMember]
        public IEnumerable<OccupiedInstance> OccupiedInstances { get; private set; }
    }
}