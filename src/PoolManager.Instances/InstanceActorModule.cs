using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Instances;

namespace PoolManager.Instances
{
    public class InstanceActorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IInstanceRepository>().To<InstanceRepository>();
            Bind<IServiceInstanceProxy>().To<ServiceInstanceProxy>();
            Bind<SDK.Partitions.IPartitionProxy>().To<SDK.Partitions.PartitionProxy>();
            Bind<IPartitionProxy>().To<PartitionProxy>();
            Kernel
                .WithCommandHandler<CreateServiceHandler, CreateService, CreateServiceResult>()
                .WithCommandHandler<DeleteServiceHandler, DeleteService>();
        }
    }
}
