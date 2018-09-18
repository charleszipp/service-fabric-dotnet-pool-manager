using Ninject;
using Ninject.Modules;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Instances.States;

namespace PoolManager.Domains.Instances
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithInstances(this IKernel kernel)
        {
            kernel.Load(new InstancesDomainModule());
            return kernel;
        }
    }

    public class InstancesDomainModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IInstanceStateProvider>().ToMethod(ctx => 
                new InstanceStateProvider(
                    new InstanceStateIdle(), 
                    new InstanceStateVacant(), 
                    new InstanceStateOccupied()                    
                    )
                );
            Bind<InstanceContext>().ToSelf();
            Kernel
                .WithCommandHandler<StartInstanceHandler, StartInstance>()
                .WithCommandHandler<OccupyInstanceHandler, OccupyInstance>()
                .WithCommandHandler<ReportActivityHandler, ReportActivity, ReportActivityResult>()
                .WithCommandHandler<VacateInstanceHandler, VacateInstance>()
                .WithCommandHandler<RemoveInstanceHandler, RemoveInstance>();
        }
    }
}