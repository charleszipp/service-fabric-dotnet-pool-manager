using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request);
        Task<GetInstanceResponse> GetInstanceAsync(string serviceTypeUri, GetInstanceRequest request);
        Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request);
        Task DeletePoolAsync(string serviceTypeUri);
        Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri);
    }
}