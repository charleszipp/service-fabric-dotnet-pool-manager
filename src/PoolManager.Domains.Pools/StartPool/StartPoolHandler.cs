using PoolManager.Core.Mediators.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public class StartPoolHandler : IHandleCommand<StartPool>
    {
        private readonly IPoolsRepository repository;
        private readonly IHandleCommand<EnsurePoolSize> ensurePoolSize;

        public StartPoolHandler(IPoolsRepository repository, IHandleCommand<EnsurePoolSize> ensurePoolSize)
        {
            this.repository = repository;
            this.ensurePoolSize = ensurePoolSize;
        }

        public async Task ExecuteAsync(StartPool command, CancellationToken cancellationToken)
        {
            await repository.SetConfigurationAsync(
                    command.ServiceTypeUri,
                    command.IsServiceStateful,
                    command.HasPersistedState,
                    command.MinReplicas,
                    command.TargetReplicas,
                    command.PartitionScheme,
                    command.MaxPoolSize,
                    command.IdleServicesPoolSize,
                    command.ServicesAllocationBlockSize,
                    command.ExpirationQuanta,
                    cancellationToken);

            await ensurePoolSize.ExecuteAsync(new EnsurePoolSize(), cancellationToken);
        }
    }
}
