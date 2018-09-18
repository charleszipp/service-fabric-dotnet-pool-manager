using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using System.Fabric;

namespace PoolManager.Core
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithCore(this IKernel kernel, ServiceContext serviceContext, IActorStateManager stateManager)
        {
            kernel.Load(new CoreModule(serviceContext, stateManager));
            return kernel;
        }
    }
}