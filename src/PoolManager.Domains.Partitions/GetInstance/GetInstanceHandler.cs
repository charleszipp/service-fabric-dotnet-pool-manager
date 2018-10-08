using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using PoolManager.Domains.Partitions.Interfaces;
using PoolManager.Domains.Pools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Partitions
{
    public class GetInstanceHandler : IHandleCommand<GetInstance, GetInstanceResult>
    {
        private readonly IHandleCommand<PopVacantInstance, PopVacantInstanceResult> getNextVacantInstance;
        private readonly IHandleCommand<OccupyInstance, OccupyInstanceResult> occupyInstance;
        private readonly IPartitionRepository partitions;

        public GetInstanceHandler(IPartitionRepository partitions,
            IHandleCommand<PopVacantInstance, PopVacantInstanceResult> getNextVacantInstance,
            IHandleCommand<OccupyInstance, OccupyInstanceResult> occupyInstance)
        {
            this.partitions = partitions;
            this.getNextVacantInstance = getNextVacantInstance;
            this.occupyInstance = occupyInstance;
        }

        public async Task<GetInstanceResult> ExecuteAsync(GetInstance command, CancellationToken cancellationToken)
        {
            var serviceUri = await partitions.TryGetOccupiedInstanceUriAsync(command.ServiceTypeUri, command.InstanceName, cancellationToken);

            if (serviceUri != null)
                return new GetInstanceResult(serviceUri);

            var nextVacantInstance = await getNextVacantInstance.ExecuteAsync(new PopVacantInstance(command.ServiceTypeUri), cancellationToken);
            if (nextVacantInstance.InstanceId.HasValue)
            {
                // todo: if occupy fails or takes over a certain time, mark the instance for deletion and retry
                var occupiedInstance = await occupyInstance.ExecuteAsync(new OccupyInstance(nextVacantInstance.InstanceId.Value, command.PartitionId, command.InstanceName), cancellationToken);
                await partitions.SetOccupiedInstanceAsync(command.ServiceTypeUri, command.InstanceName, nextVacantInstance.InstanceId.Value, occupiedInstance.ServiceName);
                return new GetInstanceResult(occupiedInstance.ServiceName);
            }

            throw new ArgumentException("Unable to find a mapped instance for given pool and name");
        }
    }
}