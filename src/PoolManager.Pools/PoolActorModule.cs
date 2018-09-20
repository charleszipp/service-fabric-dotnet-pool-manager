using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Pools;
using PoolManager.SDK.Instances;

namespace PoolManager.Pools
{
    public class PoolActorModule : NinjectModule
    {
        private readonly IGuidGetter guidGetter;

        public PoolActorModule(IGuidGetter guidGetter = null)
        {
            this.guidGetter = guidGetter;
        }

        public override void Load()
        {
            if (guidGetter == null)
                Bind<IGuidGetter>().To<GuidGetter>();
            else
                Bind<IGuidGetter>().ToConstant(guidGetter);
            Bind<IInstanceProxy>().To<InstanceProxy>();
            Bind<IPoolsRepository>().To<PoolsRepository>();
            Kernel.WithCommandHandler<StartInstanceHandler, StartInstance, StartInstanceResult>();
        }
    }
}