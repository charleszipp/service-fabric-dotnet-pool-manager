using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public interface IPoolsRepository
    {
        Task<Guid?> PopVacantInstance(CancellationToken cancellationToken);
        Task SetConfigurationAsync(string serviceTypeUri, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, PartitionSchemeDescription partitionScheme, int maxPoolSize, int idleServicesPoolSize, int servicesAllocationBlockSize, TimeSpan expirationQuanta, CancellationToken cancellationToken);
        Task<int> GetVacantInstanceTargetAsync(CancellationToken cancellationToken);
        Task<int> GetVacantInstanceCountAsync(CancellationToken cancellationToken);
        Task<int> GetAllocationBlockSizeAsync(CancellationToken cancellationToken);
        Task PushVacantInstanceAsync(Guid instanceId, CancellationToken cancellationToken);
        Task<GetPoolConfigurationResult> GetPoolConfigurationAsync(CancellationToken cancellation);
    }
}
