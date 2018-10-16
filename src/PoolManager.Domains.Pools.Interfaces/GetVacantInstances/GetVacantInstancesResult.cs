using System;
using System.Collections.Generic;

namespace PoolManager.Domains.Pools.Interfaces
{
    public class GetVacantInstancesResult
    {
        public GetVacantInstancesResult(IEnumerable<Guid> vacantInstances)
        {
            VacantInstances = vacantInstances;
        }

        public IEnumerable<Guid> VacantInstances { get; }
    }
}
