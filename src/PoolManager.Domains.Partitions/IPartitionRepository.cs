using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Partitions
{
    public interface IPartitionRepository
    {
        Task SetOccupiedInstanceAsync(string serviceTypeUri, string instanceName, Guid instanceId, Uri serviceName);
        Task<Uri> TryGetOccupiedInstanceUriAsync(string serviceTypeUri, string instanceName, CancellationToken cancellationToken);
    }
}
