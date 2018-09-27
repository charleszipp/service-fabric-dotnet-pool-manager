using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Responses
{
    [DataContract]
    public class GetOccupiedInstancesResponse
    {
        public GetOccupiedInstancesResponse(IEnumerable<OccupiedInstance> occupiedInstances)
        {
            OccupiedInstances = occupiedInstances;
        }

        [DataMember]
        public IEnumerable<OccupiedInstance> OccupiedInstances { get; private set; }
    }
}