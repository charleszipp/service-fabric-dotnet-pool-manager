using System.Threading;
using System.Threading.Tasks;
using PoolManager.Core.Mediators.Commands;

namespace PoolManager.Domains.Pools
{
    public class PushVacantInstanceHandler : IHandleCommand<PushVacantInstance>
    {
        private readonly IPoolsRepository repository;
        private readonly IHandleCommand<StartInstance, StartInstanceResult> startInstance;

        public PushVacantInstanceHandler(IPoolsRepository repository, IHandleCommand<StartInstance, StartInstanceResult> startInstance)
        {
            this.repository = repository;
            this.startInstance = startInstance;
        }

        public async Task ExecuteAsync(PushVacantInstance command, CancellationToken cancellationToken)
        {
            var result = await startInstance.ExecuteAsync(new StartInstance(), cancellationToken);
            await repository.PushVacantInstanceAsync(result.InstanceId, cancellationToken);
        }
    }
}
