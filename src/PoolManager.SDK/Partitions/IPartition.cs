using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System.Threading.Tasks;

namespace PoolManager.SDK.Partitions
{
    public interface IPartition : IActor
    {
        Task CleanupDeletedInstancesAsync();

        Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request);

        Task VacateInstanceAsync(VacateInstanceRequest request);
    }
}