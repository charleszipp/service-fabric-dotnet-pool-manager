using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request);
    }
}