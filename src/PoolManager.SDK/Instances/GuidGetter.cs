using System;

namespace PoolManager.SDK.Instances
{
    public class GuidGetter : IGuidGetter
    {
        public Guid GetAGuid()
        {
            return Guid.NewGuid();
        }
    }
}