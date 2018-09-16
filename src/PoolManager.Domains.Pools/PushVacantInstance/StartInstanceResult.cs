using System;

namespace PoolManager.Domains.Pools
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
