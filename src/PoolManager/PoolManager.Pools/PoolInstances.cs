using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace PoolManager.Pools
{
    [Serializable]
    [DataContract]
    public class PoolInstances
    {
        [DataMember]
        public ConcurrentQueue<Guid> VacantInstances { get; set; } = new ConcurrentQueue<Guid>();

        [DataMember]
        public ConcurrentDictionary<string, Guid> OccupiedInstances { get; set; } = new ConcurrentDictionary<string, Guid>();
    }
}