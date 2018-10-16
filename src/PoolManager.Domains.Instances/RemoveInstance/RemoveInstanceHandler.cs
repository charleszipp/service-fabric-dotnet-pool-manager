using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class RemoveInstanceHandler : IHandleCommand<RemoveInstance>
    {
        private readonly IInstanceRepository repository;
        private readonly IServiceInstanceProxy serviceInstance;

        public RemoveInstanceHandler(IInstanceRepository repository, IServiceInstanceProxy serviceInstance)
        {
            this.repository = repository;
            this.serviceInstance = serviceInstance;
        }

        public async Task ExecuteAsync(RemoveInstance command, CancellationToken cancellationToken)
        {
            var serviceUri = await repository.GetServiceUriAsync(cancellationToken);
            await serviceInstance.DeleteAsync(serviceUri, cancellationToken);
        }
    }
}