using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Responses
{
    [DataContract]
    public class GetInstancesResponse
    {
        public GetInstancesResponse(string serviceTypeUri, IEnumerable<Guid> vacantInstances, IEnumerable<Guid> occupiedInstances)
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
        public IEnumerable<Guid> OccupiedInstances { get; private set; }
    }
}