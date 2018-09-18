using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Pools;
using PoolManager.SDK.Instances;

namespace PoolManager.Pools
{
    public class PoolActorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGuidGetter>().To<GuidGetter>();
            Bind<IInstanceProxy>().To<InstanceProxy>();
            Kernel.WithCommandHandler<StartInstanceHandler, StartInstance, StartInstanceResult>();
        }
    }
}