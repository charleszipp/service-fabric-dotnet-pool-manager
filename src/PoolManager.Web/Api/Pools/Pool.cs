using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PoolManager.Web.Api.Pools
{
    [DataContract]
    public class Pool
    {
        public Pool(PoolConfiguration configuration, IEnumerable<Guid> vacantInstances)
        {
            Configuration = configuration;
            VacantInstances = vacantInstances;
        }

        [DataMember]
        public PoolConfiguration Configuration { get; private set; }

        [DataMember]
        public IEnumerable<Guid> VacantInstances { get; private set; }
    }
}
