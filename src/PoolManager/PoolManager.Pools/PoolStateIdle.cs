using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolStateIdle : PoolState
    {
        public override Task<Guid> GetAsync(PoolContext context, GetInstanceRequest request) => 
            throw new Exception("GetAsync called for idle pool. The pool must be started before instances can be resolved");
    }
}