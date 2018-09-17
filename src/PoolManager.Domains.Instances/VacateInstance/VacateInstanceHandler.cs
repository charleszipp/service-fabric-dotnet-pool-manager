using PoolManager.Core.Mediators.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class VacateInstanceHandler : IHandleCommand<VacateInstance>
    {
        private readonly IInstanceRepository repository;
        private readonly IServiceInstanceProxy serviceInstance;

        public VacateInstanceHandler(IInstanceRepository repository, IServiceInstanceProxy serviceInstance)
        {
            this.repository = repository;
            this.serviceInstance = serviceInstance;
        }

        public async Task ExecuteAsync(VacateInstance command, CancellationToken cancellationToken)
        {
            var serviceUri = await repository.GetServiceUriAsync(cancellationToken);
            await serviceInstance.VacateAsync(serviceUri);
            await repository.UnsetServiceInstanceName(cancellationToken);
        }
    }
}