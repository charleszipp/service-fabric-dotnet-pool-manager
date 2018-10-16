using Ninject;
using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Pools.Interfaces;

namespace PoolManager.Domains.Pools
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithPools(this IKernel kernel)
        {
            kernel.Load(new PoolsModule());
            return kernel;
        }
    }

    public class PoolsModule : NinjectModule
    {
        public PoolsModule()
        {
        }

        public override void Load()
        {
            Kernel
                .WithCommandHandler<StartPoolHandler, StartPool>()
                .WithCommandHandler<EnsurePoolSizeHandler, EnsurePoolSize>()
                .WithCommandHandler<PopVacantInstanceHandler, PopVacantInstance, PopVacantInstanceResult>()
                .WithCommandHandler<PushVacantInstanceHandler, PushVacantInstance>()
                .WithQueryHandler<GetPoolConfigurationHandler, GetPoolConfiguration, GetPoolConfigurationResult>()
                .WithQueryHandler<GetVacantInstancesHandler, GetVacantInstances, GetVacantInstancesResult>();
        }
    }
}