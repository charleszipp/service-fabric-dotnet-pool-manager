using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.Web.Api.Pools
{
    [DataContract]
    public class Pool
    {
        public Pool(PoolConfiguration configuration, IEnumerable<Guid> vacantInstances, IEnumerable<Guid> occupiedInstances)
        {
            Configuration = configuration;
            VacantInstances = vacantInstances;
            OccupiedInstances = occupiedInstances;
        }

        [DataMember]
        public PoolConfiguration Configuration { get; private set; }

        [DataMember]
        public IEnumerable<Guid> VacantInstances { get; private set; }

        [DataMember]
        public IEnumerable<Guid> OccupiedInstances { get; private set; }
    }
}
