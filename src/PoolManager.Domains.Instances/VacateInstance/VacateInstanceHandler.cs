using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class VacateInstanceHandler : IHandleCommand<VacateInstance>
    {
        private readonly IInstanceRepository repository;
        private readonly IServiceInstanceProxy serviceInstance;
        private readonly IHandleCommand<DeleteService> deleteService;

        public VacateInstanceHandler(IInstanceRepository repository, IServiceInstanceProxy serviceInstance, IHandleCommand<DeleteService> deleteService)
        {
            this.repository = repository;
            this.serviceInstance = serviceInstance;
            this.deleteService = deleteService;
        }

        public async Task ExecuteAsync(VacateInstance command, CancellationToken cancellationToken)
        {
            var serviceUri = await repository.GetServiceUriAsync(cancellationToken);
            await serviceInstance.VacateAsync(serviceUri);
            await repository.UnsetServiceInstanceName(cancellationToken);
            await deleteService.ExecuteAsync(new DeleteService(serviceUri), cancellationToken);
        }
    }
}