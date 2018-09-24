using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Responses
{
    [DataContract]
    public class GetOccupiedInstancesResponse
    {
        public GetOccupiedInstancesResponse(IEnumerable<Guid> occupiedInstances)
        {
            OccupiedInstances = occupiedInstances;
        }

        [DataMember]
        public IEnumerable<Guid> OccupiedInstances { get; private set; }
    }
}