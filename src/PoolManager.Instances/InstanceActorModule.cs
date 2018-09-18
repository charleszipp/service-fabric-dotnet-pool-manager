using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoolManager.Instances
{
    public class InstanceActorModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IInstanceRepository>().To<InstanceRepository>();
            Bind<IServiceInstanceProxy>().To<ServiceInstanceProxy>();
            Bind<IPartitionProxy>().To<PartitionProxy>();
            Kernel
                .WithCommandHandler<CreateServiceHandler, CreateService, CreateServiceResult>();
        }
    }
}
