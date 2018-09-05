using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPool : IActor
    {
        Task StartAsync(StartPoolRequest request);
        Task StopAsync();
        Task<GetInstanceResponse> GetAsync(GetInstanceRequest request);
        Task VacateInstanceAsync(VacateInstanceRequest request);
        Task<ConfigurationResponse> GetConfigurationAsync();
        Task<bool> IsActive();
    }
}