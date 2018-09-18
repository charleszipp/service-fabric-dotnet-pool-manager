using System;
using System.Threading;
using System.Threading.Tasks;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Pools;
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

        public Task<StartInstanceResult> ExecuteAsync(StartInstance command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //todo: implement start instance from pool actor
            //var config = await repository.GetPoolConfigurationAsync(cancellationToken);
            //var instanceId = await instances.StartAsync(new SDK.Instances.Requests.StartInstanceRequest(
            //    ,
            //    config.ServiceTypeUri,
            //    config.IsServiceStateful,
            //    config.HasPersistedState,
            //    config.MinReplicaSetSize,
            //    config.TargetReplicasetSize,
            //    (SDK.PartitionSchemeDescription)Enum.Parse(typeof(SDK.PartitionSchemeDescription), config.PartitionScheme.ToString()),
            //    config.ExpirationQuanta
            //    ));
            //return new StartInstanceResult(instanceId);
        }
    }
}