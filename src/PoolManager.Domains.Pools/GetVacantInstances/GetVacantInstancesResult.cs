using System;
using System.Collections.Generic;

namespace PoolManager.Domains.Pools
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
