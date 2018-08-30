using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPool : IActor
    {
        Task StartAsync(StartPoolRequest request);
        Task<Guid> GetAsync(GetInstanceRequest request);
        Task VacateInstanceAsync(VacateInstanceRequest request);
    }
}