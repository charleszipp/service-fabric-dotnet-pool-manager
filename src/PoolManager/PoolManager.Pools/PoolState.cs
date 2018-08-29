using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public abstract class PoolState
    {
        public PoolStates State { get; }

        public virtual async Task<PoolState> StartAsync(PoolContext context, StartPoolRequest request)
        {
            PoolConfiguration config = new PoolConfiguration
            {
                ExpirationQuanta = request.ExpirationQuanta,
                HasPersistedState = request.HasPersistedState,
                IdleServicesPoolSize = request.IdleServicesPoolSize,
                IsServiceStateful = request.IsServiceStateful,
                MaxPoolSize = request.MaxPoolSize,
                MinReplicaSetSize = request.MinReplicas,
                PartitionScheme = request.PartitionScheme,
                ServicesAllocationBlockSize = request.ServicesAllocationBlockSize,
                ServiceTypeUri = context.ServiceTypeUri,
                TargetReplicasetSize = request.TargetReplicas
            };

            //todo: make sure the service type provided exists

            await context.SetPoolConfigurationAsync(config);
            await context.EnsurePoolSizeAsync(config);
            return context.PoolStates.Get(PoolStates.Active);
        }

        public abstract Task<Guid> GetAsync(PoolContext context, GetInstanceRequest request);

        public abstract Task VacateInstanceAsync(PoolContext poolContext, VacateInstanceRequest request);
    }
}