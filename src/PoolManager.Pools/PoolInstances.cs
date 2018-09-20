using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.Pools
{
    [Serializable]
    [DataContract]
    public class PoolInstances
    {
        [DataMember]
        public Queue<Guid> VacantInstances { get; private set; } = new Queue<Guid>();
    }
}