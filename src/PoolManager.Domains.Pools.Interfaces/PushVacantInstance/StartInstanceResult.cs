using System;

namespace PoolManager.Domains.Pools.Interfaces
{
    public class StartInstanceResult
    {
        public StartInstanceResult(Guid instanceId)
        {
            InstanceId = instanceId;
        }

        public Guid InstanceId { get; }
    }
}
