using PoolManager.Core.Mediators.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class OccupyInstanceHandler : IHandleCommand<OccupyInstance>
    {
        private readonly IInstanceRepository repository;
        private readonly IServiceInstanceProxy instanceProxy;

        public OccupyInstanceHandler(IInstanceRepository repository, IServiceInstanceProxy instanceProxy)
        {
            this.repository = repository;
            this.instanceProxy = instanceProxy;
        }

        public async Task ExecuteAsync(OccupyInstance command, CancellationToken cancellationToken)
        {
            Uri serviceUri = await repository.GetServiceUriAsync(cancellationToken);
            await instanceProxy.OccupyAsync(serviceUri, command.InstanceId, command.InstanceName);
            await Task.WhenAll(
                repository.SetServiceInstanceName(command.InstanceName, cancellationToken),
                repository.SetPartitionIdAsync(command.PartitionId, cancellationToken)
            );
        }
    }
}
