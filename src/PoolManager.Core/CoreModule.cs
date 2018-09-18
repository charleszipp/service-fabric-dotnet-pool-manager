using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Ninject;
using Ninject.Modules;
using System.Fabric;

namespace PoolManager.Core
{
    public class CoreModule : NinjectModule
    {
        private readonly ServiceContext serviceContext;
        private readonly IActorStateManager stateManager;

        public CoreModule(ServiceContext serviceContext, IActorStateManager stateManager)
        {
            this.serviceContext = serviceContext;
            this.stateManager = stateManager;
        }

        public override void Load()
        {
            Bind<ServiceContext>().ToConstant(serviceContext);
            Bind<TelemetryClient>().ToSelf();
            Bind<IActorProxyFactory>().ToMethod(ctx =>
                new CorrelatingActorProxyFactory(
                    ctx.Kernel.Get<ServiceContext>(),
                    callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                    )
                ).InSingletonScope();
            Bind<IServiceProxyFactory>().ToMethod(ctx =>
                new CorrelatingServiceProxyFactory(
                    ctx.Kernel.Get<ServiceContext>(),
                    callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                    )
                ).InSingletonScope();
            Bind<IActorStateManager>().ToConstant(stateManager);
            Bind<FabricClient>().ToSelf().InSingletonScope();
            Bind<IClusterClient>().To<ClusterClient>();
        }
    }
}