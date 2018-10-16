using Ninject.Modules;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Pools;

namespace PoolManager.Partitions
{
    public class PartitionActorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPoolProxy>().To<PoolProxy>();
            Bind<IPartitionProxy>().To<PartitionProxy>();
            Bind<IInstanceProxy>().To<InstanceProxy>();
            Bind<IGuidGetter>().To<GuidGetter>();
        }
    }
}
