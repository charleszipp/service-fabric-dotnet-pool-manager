using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Responses
{
    [DataContract]
    public class GetVacantInstancesResponse
    {
        public GetVacantInstancesResponse(IEnumerable<Guid> vacantInstances)
        {
            VacantInstances = vacantInstances;
        }

        [DataMember]
        public IEnumerable<Guid> VacantInstances { get; private set; }
    }
}