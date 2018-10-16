using System;

namespace PoolManager.Domains.Pools.Interfaces
{
    public class PopVacantInstanceResult
    {
        public PopVacantInstanceResult(Guid? instanceId)
        {
            InstanceId = instanceId;
        }

        public Guid? InstanceId { get; }
    }
}
