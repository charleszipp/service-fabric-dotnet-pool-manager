using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public interface IInstanceRepository
    {
        Task SetExprirationQuantaAsync(TimeSpan expirationQuanta, CancellationToken cancellationToken);

        Task SetServiceUriAsync(Uri serviceUri, CancellationToken cancellationToken);

        Task SetServiceInstanceName(string instanceName, CancellationToken cancellationToken);

        Task<Uri> GetServiceUriAsync(CancellationToken cancellationToken);

        Task<TimeSpan> GetExpirationQuantaAsync(CancellationToken cancellationToken);

        Task SetServiceLastActiveAsync(DateTime lastActiveUtc, CancellationToken cancellationToken);

        Task UnsetServiceInstanceName(CancellationToken cancellationToken);

        Task SetInstanceStateAsync(InstanceStates state, CancellationToken cancellation);

        Task<DateTime?> GetServiceLastActiveAsync(CancellationToken cancellationToken);

        Task<InstanceStates?> GetInstanceStateAsync(CancellationToken cancellationToken);

        Task<string> GetServiceInstanceNameAsync(CancellationToken cancellationToken);

        Task<string> GetPartitionIdAsync(CancellationToken cancellationToken);
    }
}