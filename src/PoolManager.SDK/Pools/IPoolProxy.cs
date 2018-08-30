using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request);

        Task<GetInstanceResponse> GetInstanceAsync(string serviceTypeUri, GetInstanceRequest request);

        Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request);

        Task DeletePoolAsync(string serviceTypeUri);
    }
}