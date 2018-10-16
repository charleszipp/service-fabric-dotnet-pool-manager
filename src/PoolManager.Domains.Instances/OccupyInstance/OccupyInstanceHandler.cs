using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class OccupyInstanceHandler : IHandleCommand<OccupyInstance, OccupyInstanceResult>
    {
        private readonly IInstanceRepository repository;
        private readonly IServiceInstanceProxy instanceProxy;

        public OccupyInstanceHandler(IInstanceRepository repository, IServiceInstanceProxy instanceProxy)
        {
            this.repository = repository;
            this.instanceProxy = instanceProxy;
        }

        public async Task<OccupyInstanceResult> ExecuteAsync(OccupyInstance command, CancellationToken cancellationToken)
        {
            Uri serviceUri = await repository.GetServiceUriAsync(cancellationToken);
            await instanceProxy.OccupyAsync(serviceUri, command.InstanceId, command.InstanceName);
            await Task.WhenAll(
                repository.SetServiceInstanceName(command.InstanceName, cancellationToken),
                repository.SetPartitionIdAsync(command.PartitionId, cancellationToken),
                repository.SetServiceLastActiveAsync(DateTime.UtcNow, cancellationToken)
            );

            return new OccupyInstanceResult(serviceUri);
        }
    }
}
