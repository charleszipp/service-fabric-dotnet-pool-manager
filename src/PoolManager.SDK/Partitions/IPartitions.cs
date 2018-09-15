using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System.Threading.Tasks;

namespace PoolManager.Partitions.Interfaces
{
    public interface IPartitions : IActor
    {
        Task CleanupDeletedInstancesAsync();

        Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request);

        Task VacateInstanceAsync(VacateInstanceRequest request);
    }
}