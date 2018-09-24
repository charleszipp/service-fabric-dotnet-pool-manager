using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;
using System.Threading;
using System.Collections.Generic;

namespace PoolManager.SDK.Pools
{
    public interface IPoolProxy
    {
        Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request);
        Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri);
        Task<PopVacantInstanceResponse> PopVacantInstanceAsync(string serviceTypeUri, PopVacantInstanceRequest request);
        Task<GetVacantInstancesResponse> GetVacantInstancesAsync(string serviceTypeUri);
        Task<GetInstancesResponse> GetInstancesAsync(string serviceTypeUri, CancellationToken cancellationToken);
        Task<IEnumerable<GetInstancesResponse>> GetInstancesAsync(CancellationToken cancellationToken);
    }
}