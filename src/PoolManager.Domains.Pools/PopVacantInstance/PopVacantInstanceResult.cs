using System;

namespace PoolManager.Domains.Pools
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
