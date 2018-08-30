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
        public ConcurrentQueue<Guid> VacantInstances { get; private set; } = new ConcurrentQueue<Guid>();

        [DataMember]
        public ConcurrentDictionary<string, Guid> OccupiedInstances { get; private set; } = new ConcurrentDictionary<string, Guid>();

        [DataMember]
        public ConcurrentQueue<Guid> RemovedInstances { get; private set; } = new ConcurrentQueue<Guid>();
    }
}