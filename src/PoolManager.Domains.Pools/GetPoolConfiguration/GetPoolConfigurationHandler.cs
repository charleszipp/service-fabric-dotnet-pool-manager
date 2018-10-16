using PoolManager.Core.Mediators.Queries;
using PoolManager.Domains.Pools.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public class GetPoolConfigurationHandler : IHandleQuery<GetPoolConfiguration, GetPoolConfigurationResult>
    {
        private readonly IPoolsRepository repository;

        public GetPoolConfigurationHandler(IPoolsRepository repository) => 
            this.repository = repository;

        public Task<GetPoolConfigurationResult> ExecuteAsync(GetPoolConfiguration query, CancellationToken cancellationToken) => 
            repository.TryGetPoolConfigurationAsync(cancellationToken);
    }
}