using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Ninject;
using System.Fabric;

namespace PoolManager.Core
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithCore(this IKernel kernel, 
            ServiceContext serviceContext, 
            IActorStateManager stateManager,
            IClusterClient clusterClient = null, 
            TelemetryClient telemetry = null,
            IActorProxyFactory actorProxyFactory = null,
            IServiceProxyFactory serviceProxyFactory = null)
        {
            kernel.Load(new CoreModule(serviceContext, stateManager, clusterClient, telemetry, actorProxyFactory, serviceProxyFactory));
            return kernel;
        }
    }
}