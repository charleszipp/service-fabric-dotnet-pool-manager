using System;
using System.Threading;
using System.Threading.Tasks;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Pools.Interfaces;
using PoolManager.SDK.Instances;

namespace PoolManager.Pools
{
    internal class StartInstanceHandler : IHandleCommand<StartInstance, StartInstanceResult>
    {
        private readonly IInstanceProxy instances;
        private readonly IPoolsRepository repository;

        public StartInstanceHandler(IInstanceProxy instances, IPoolsRepository repository)
        {
            this.instances = instances;
            this.repository = repository;
        }

        public async Task<StartInstanceResult> ExecuteAsync(StartInstance command, CancellationToken cancellationToken)
        {
            var config = await repository.TryGetPoolConfigurationAsync(cancellationToken);
            if (config == null)
                throw new InvalidOperationException("Pool configuration is not set. Unable to start instance");
            else
            {
                var instanceId = await instances.StartAsync(new SDK.Instances.Requests.StartInstanceRequest(
                    config.ServiceTypeUri,
                    config.IsServiceStateful,
                    config.HasPersistedState,
                    config.MinReplicaSetSize,
                    config.TargetReplicasetSize,
                    (SDK.PartitionSchemeDescription)Enum.Parse(typeof(SDK.PartitionSchemeDescription), config.PartitionScheme.ToString()),
                    config.ExpirationQuanta
                    ));
                return new StartInstanceResult(instanceId);
            }
        }
    }
}