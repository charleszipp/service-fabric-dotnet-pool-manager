using PoolManager.Core.Mediators.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public class PopVacantInstanceHandler : IHandleCommand<PopVacantInstance, PopVacantInstanceResult>
    {
        private readonly IPoolsRepository repository;

        public PopVacantInstanceHandler(IPoolsRepository repository)
        {
            this.repository = repository;
        }

        public async Task<PopVacantInstanceResult> ExecuteAsync(PopVacantInstance command, CancellationToken cancellationToken)
        {
            var nextInstanceId = await repository.PopVacantInstance(cancellationToken);
            return new PopVacantInstanceResult(nextInstanceId);
        }
    }
}
