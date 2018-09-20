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
        private readonly IClusterClient clusterClient;
        private readonly TelemetryClient telemetry;
        private readonly IActorProxyFactory actorProxyFactory;
        private readonly IServiceProxyFactory serviceProxyFactory;

        public CoreModule(ServiceContext serviceContext, 
            IActorStateManager stateManager, 
            IClusterClient clusterClient = null, 
            TelemetryClient telemetry = null,
            IActorProxyFactory actorProxyFactory = null,
            IServiceProxyFactory serviceProxyFactory = null)
        {
            this.serviceContext = serviceContext;
            this.stateManager = stateManager;
            this.clusterClient = clusterClient;
            this.telemetry = telemetry;
            this.actorProxyFactory = actorProxyFactory;
            this.serviceProxyFactory = serviceProxyFactory;
        }

        public override void Load()
        {
            Bind<ServiceContext>().ToConstant(serviceContext);

            if (telemetry == null)
                Bind<TelemetryClient>().ToSelf();
            else
                Bind<TelemetryClient>().ToConstant(telemetry);

            if (actorProxyFactory == null)
                Bind<IActorProxyFactory>().ToMethod(ctx =>
                    new CorrelatingActorProxyFactory(
                        ctx.Kernel.Get<ServiceContext>(),
                        callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                        )
                    ).InSingletonScope();
            else
                Bind<IActorProxyFactory>().ToConstant(actorProxyFactory);

            if (serviceProxyFactory == null)
                Bind<IServiceProxyFactory>().ToMethod(ctx =>
                    new CorrelatingServiceProxyFactory(
                        ctx.Kernel.Get<ServiceContext>(),
                        callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                        )
                    ).InSingletonScope();
            else
                Bind<IServiceProxyFactory>().ToConstant(serviceProxyFactory);

            Bind<IActorStateManager>().ToConstant(stateManager);

            if (clusterClient == null)
            {
                Bind<FabricClient>().ToSelf().InSingletonScope();
                Bind<IClusterClient>().To<ClusterClient>();
            }
            else
                Bind<IClusterClient>().ToConstant(clusterClient);
            
        }
    }
}