using System;

namespace PoolManager.Domains.Partitions
{
    public class GetInstanceResult
    {
        public GetInstanceResult(Uri serviceName)
        {
            ServiceName = serviceName;
        }

        public Uri ServiceName { get; }
    }
}
