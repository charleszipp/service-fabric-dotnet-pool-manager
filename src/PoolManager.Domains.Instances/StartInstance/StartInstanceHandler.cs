using PoolManager.Core.Mediators.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class StartInstanceHandler : IHandleCommand<StartInstance>
    {
        private readonly IInstanceRepository repository;
        private readonly IHandleCommand<CreateService, CreateServiceResult> createService;

        public StartInstanceHandler(IInstanceRepository repository, IHandleCommand<CreateService, CreateServiceResult> createService)
        {
            this.repository = repository;
            this.createService = createService;
        }

        public async Task ExecuteAsync(StartInstance command, CancellationToken cancellationToken)
        {
            var result = await createService.ExecuteAsync(
                new CreateService(
                    command.InstanceId,
                    command.ServiceTypeUri, 
                    command.IsServiceStateful, 
                    command.HasPersistedState, 
                    command.MinReplicas, 
                    command.TargetReplicas, 
                    command.PartitionScheme), 
                cancellationToken);

            await Task.WhenAll(
                repository.SetExprirationQuantaAsync(command.ExpirationQuanta, cancellationToken),
                repository.SetServiceUriAsync(result.ServiceUri, cancellationToken),
                repository.SetServiceTypeUriAsync(command.ServiceTypeUri, cancellationToken)
            );
        }
    }
}