using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public interface IPool : IActor
    {
        Task StartAsync(StartPoolRequest request);
        Task GetAsync(GetInstanceRequest request);
    }
}