using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request);

        Task GetInstanceAsync(string serviceTypeUri, GetInstanceRequest request);

        Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request);

        Task DeletePoolAsync(string serviceTypeUri);
    }
}