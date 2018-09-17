using PoolManager.Core.Mediators.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class CheckForExpirationHandler : IHandleCommand<CheckForExpiration>
    {
        private readonly IInstanceRepository repository;
        private readonly IPartitionProxy partitions;

        public CheckForExpirationHandler(IInstanceRepository repository, IPartitionProxy partitions)
        {
            this.repository = repository;
            this.partitions = partitions;
        }

        public async Task ExecuteAsync(CheckForExpiration command, CancellationToken cancellationToken)
        {
            var expirationQuanta = await repository.GetExpirationQuantaAsync(cancellationToken);
            var lastActive = await repository.GetServiceLastActiveAsync(cancellationToken);
            if(lastActive.HasValue)
            {
                var inactivityPeriod = command.AsOfDate.Subtract(lastActive.Value);
                if(inactivityPeriod > expirationQuanta)
                {
                    string instanceName = await repository.GetServiceInstanceNameAsync(cancellationToken);
                    string partitionId = await repository.GetPartitionIdAsync(cancellationToken);
                    await partitions.VacateInstanceAsync(partitionId, command.InstanceId, instanceName);
                }
            }
        }
    }
}
