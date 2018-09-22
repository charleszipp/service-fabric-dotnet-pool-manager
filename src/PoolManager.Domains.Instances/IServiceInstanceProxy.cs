using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public interface IServiceInstanceProxy
    {
        Task OccupyAsync(Uri serviceUri, Guid instanceId, string instanceName);
        Task VacateAsync(Uri serviceUri);
        Task DeleteAsync(Uri serviceUri, CancellationToken cancellationToken);
    }
}
