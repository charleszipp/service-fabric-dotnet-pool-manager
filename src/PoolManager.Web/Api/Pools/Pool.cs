using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PoolManager.Web.Api.Pools
{
    [DataContract]
    public class Pool
    {
        public Pool(PoolConfiguration configuration, IEnumerable<string> partitions, IEnumerable<Guid> vacantInstances, IEnumerable<OccupiedInstance> occupiedInstances)
        {
            Configuration = configuration;
            Partitions = partitions;
            PartitionsCount = partitions.Count();
            VacantInstances = vacantInstances;
            VacantInstancesCount = vacantInstances.Count();
            OccupiedInstances = occupiedInstances;
            OccupiedInstancesCount = occupiedInstances.Count();
        }
        [DataMember]
        public int PartitionsCount { get; private set; }
        [DataMember]
        public int VacantInstancesCount { get; private set; }
        [DataMember]
        public int OccupiedInstancesCount { get; private set; }
        [DataMember]
        public PoolConfiguration Configuration { get; private set; }
        [DataMember]
        public IEnumerable<string> Partitions { get; private set; }        
        [DataMember]
        public IEnumerable<Guid> VacantInstances { get; private set; }
        [DataMember]
        public IEnumerable<OccupiedInstance> OccupiedInstances { get; private set; }
    }
}
