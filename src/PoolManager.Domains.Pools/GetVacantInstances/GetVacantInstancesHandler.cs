using PoolManager.Core.Mediators.Queries;
using PoolManager.Domains.Pools.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public class GetVacantInstancesHandler : IHandleQuery<GetVacantInstances, GetVacantInstancesResult>
    {
        private readonly IPoolsRepository repository;

        public GetVacantInstancesHandler(IPoolsRepository repository)
        {
            this.repository = repository;
        }

        public async Task<GetVacantInstancesResult> ExecuteAsync(GetVacantInstances query, CancellationToken cancellationToken)
        {
            var vacantInstances = await repository.GetVacantInstances(cancellationToken);
            return new GetVacantInstancesResult(vacantInstances);
        }
    }
}
