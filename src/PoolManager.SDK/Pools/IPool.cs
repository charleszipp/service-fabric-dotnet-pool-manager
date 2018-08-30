using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.SDK.Pools
{
    public interface IPool : IActor
    {
        Task StartAsync(StartPoolRequest request);
        Task<GetInstanceResponse> GetAsync(GetInstanceRequest request);
        Task VacateInstanceAsync(VacateInstanceRequest request);
        Task<ConfigurationResponse> GetConfigurationAsync();
    }
}