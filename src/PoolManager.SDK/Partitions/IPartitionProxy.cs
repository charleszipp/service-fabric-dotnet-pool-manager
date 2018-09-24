using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.SDK.Partitions
{
    public interface IPartitionProxy
    {
        Task<GetInstanceResponse> GetInstanceAsync(string partitionId, GetInstanceRequest request);
        Task VacateInstanceAsync(string partitionId, VacateInstanceRequest request);
        Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(string partitionId, string serviceTypeUri);
        Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(string serviceTypeUri, CancellationToken cancellationToken);
    }
}
