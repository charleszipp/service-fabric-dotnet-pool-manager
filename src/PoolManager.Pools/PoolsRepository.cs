using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using PoolManager.Domains.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolsRepository : IPoolsRepository
    {
        private readonly IActorStateManager stateManager;
        private const string PoolInstancesStateName = "pool-instances";
        private const string PoolConfigurationStateName = "pool-configuration";

        public PoolsRepository(IActorStateManager stateManager) =>
            this.stateManager = stateManager;

        public async Task<Guid?> PopVacantInstance(CancellationToken cancellationToken)
        {
            Guid? nextInstanceId = null;
            var instances = await GetPoolInstancesStateAsync(cancellationToken);
            if (instances.VacantInstances.Any())
                nextInstanceId = instances.VacantInstances.Dequeue();
            return nextInstanceId;
        }

        public async Task SetConfigurationAsync(bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, PartitionSchemeDescription partitionScheme, int maxPoolSize, int idleServicesPoolSize, int servicesAllocationBlockSize, TimeSpan expirationQuanta, CancellationToken cancellationToken)
        {
            PoolConfiguration config = new PoolConfiguration
            {
                ExpirationQuanta = expirationQuanta,
                HasPersistedState = hasPersistedState,
                IdleServicesPoolSize = idleServicesPoolSize,
                IsServiceStateful = isServiceStateful,
                MaxPoolSize = maxPoolSize,
                MinReplicaSetSize = minReplicas,
                PartitionScheme = partitionScheme,
                ServicesAllocationBlockSize = servicesAllocationBlockSize,
                TargetReplicasetSize = targetReplicas
            };

            await SetConfigurationStateAsync(config, cancellationToken);
        }

        public async Task<int> GetVacantInstanceTargetAsync(CancellationToken cancellationToken) =>
            (await GetConfigurationStateAsync(cancellationToken)).IdleServicesPoolSize;

        public async Task<int> GetVacantInstanceCountAsync(CancellationToken cancellationToken) =>
            (await GetPoolInstancesStateAsync(cancellationToken)).VacantInstances.Count;

        public async Task<int> GetAllocationBlockSizeAsync(CancellationToken cancellationToken) =>
            (await GetConfigurationStateAsync(cancellationToken)).ServicesAllocationBlockSize;

        public async Task PushVacantInstanceAsync(Guid instanceId, CancellationToken cancellationToken)
        {
            var instances = await GetPoolInstancesStateAsync(cancellationToken);
            instances.VacantInstances.Enqueue(instanceId);
            await SetPoolInstancesAsync(instances, cancellationToken);
        }

        private async Task<PoolConfiguration> GetConfigurationStateAsync(CancellationToken cancellationToken)
        {
            var config = await TryGetConfigurationStateAsync(cancellationToken);
            if (!config.HasValue)
                throw new InvalidOperationException("No configuration found for pool");
            return config.Value;
        }

        private Task<ConditionalValue<PoolConfiguration>> TryGetConfigurationStateAsync(CancellationToken cancellationToken) =>
            stateManager.TryGetStateAsync<PoolConfiguration>(PoolConfigurationStateName, cancellationToken);

        private Task SetConfigurationStateAsync(PoolConfiguration poolConfiguration, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(PoolConfigurationStateName, poolConfiguration, cancellationToken);

        private Task<PoolInstances> GetPoolInstancesStateAsync(CancellationToken cancellationToken) =>
            stateManager.GetOrAddStateAsync(PoolInstancesStateName, CreateNewPoolInstances(), cancellationToken);

        private PoolInstances CreateNewPoolInstances() => 
            new PoolInstances();

        private Task SetPoolInstancesAsync(PoolInstances instances, CancellationToken cancellationToken) =>
            stateManager.SetStateAsync(PoolInstancesStateName, instances, cancellationToken);

        public Task SetConfigurationAsync(string serviceTypeUri, bool isServiceStateful, bool hasPersistedState, int minReplicas, int targetReplicas, PartitionSchemeDescription partitionScheme, int maxPoolSize, int idleServicesPoolSize, int servicesAllocationBlockSize, TimeSpan expirationQuanta, CancellationToken cancellationToken)
        {
            return SetConfigurationStateAsync(
                new PoolConfiguration
                {
                    ExpirationQuanta = expirationQuanta,
                    HasPersistedState = hasPersistedState,
                    IdleServicesPoolSize = idleServicesPoolSize,
                    IsServiceStateful = isServiceStateful,
                    MaxPoolSize = maxPoolSize,
                    MinReplicaSetSize = minReplicas,
                    PartitionScheme = partitionScheme,
                    ServicesAllocationBlockSize = servicesAllocationBlockSize,
                    ServiceTypeUri = serviceTypeUri,
                    TargetReplicasetSize = targetReplicas
                }, 
                cancellationToken);
        }

        public async Task<GetPoolConfigurationResult> TryGetPoolConfigurationAsync(CancellationToken cancellation)
        {
            var config = await TryGetConfigurationStateAsync(cancellation);
            if (config.HasValue)
                return new GetPoolConfigurationResult(
                    config.Value.ExpirationQuanta,
                    config.Value.HasPersistedState,
                    config.Value.IdleServicesPoolSize,
                    config.Value.IsServiceStateful,
                    config.Value.MaxPoolSize,
                    config.Value.MinReplicaSetSize,
                    config.Value.PartitionScheme,
                    config.Value.ServicesAllocationBlockSize,
                    config.Value.ServiceTypeUri,
                    config.Value.TargetReplicasetSize
                    );
            else
                return null;
        }

        public async Task<IEnumerable<Guid>> GetVacantInstances(CancellationToken cancellationToken)
        {
            var poolInstances = await GetPoolInstancesStateAsync(cancellationToken);
            return poolInstances.VacantInstances.ToList();
        }
    }
}