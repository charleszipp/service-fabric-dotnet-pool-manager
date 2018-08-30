using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolStateIdle : PoolState
    {
        public override PoolStates State => PoolStates.Idle;

        public override Task<GetInstanceResponse> GetAsync(PoolContext context, GetInstanceRequest request) => 
            throw new Exception("GetAsync called for idle pool. The pool must be started before instances can be resolved");

        public override Task VacateInstanceAsync(PoolContext poolContext, VacateInstanceRequest request) => 
            throw new Exception("VacateInstanceAsync called for idle pool. The pool must be started before instances can be vacated");
    }
}