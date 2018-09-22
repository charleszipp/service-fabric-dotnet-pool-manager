using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System.Threading.Tasks;

namespace PoolManager.SDK.Partitions
{
    public interface IPartitionProxy
    {
        Task<GetInstanceResponse> GetInstanceAsync(string partitionId, GetInstanceRequest request);
        Task VacateInstanceAsync(string partitionId, VacateInstanceRequest request);
    }
}
