using System.Fabric;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using PoolManager.Core;
using PoolManager.Core.Mediators.Builders;
using PoolManager.Core.Mediators.Resolvers;
using PoolManager.Domains.Pools;
using PoolManager.SDK.Instances;

namespace PoolManager.Pools
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Pool>((context, actorType) =>
            {
                var kernel = new StandardKernel();
                kernel.Bind<ServiceContext>().ToConstant(context);
                kernel.Bind<TelemetryClient>().ToSelf();
                kernel.Bind<IGuidGetter>().To<GuidGetter>();
                kernel.Bind<IActorProxyFactory>().ToMethod(ctx =>
                    new CorrelatingActorProxyFactory(
                        ctx.Kernel.Get<ServiceContext>(),
                        callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                        )
                    ).InSingletonScope();
                kernel.Bind<IInstanceProxy>().To<InstanceProxy>();

                var mediatorBuilder = new MediatorBuilder(new NinjectDependencyResolver(kernel))
                    .WithCommandHandler<StartInstanceHandler, StartInstance, StartInstanceResult>()
                    .WithPools();                

                return new PoolsActorService(
                    context,
                    actorType,
                    "PoolActorServiceEndpoint",
                    (svc, id) =>
                        new Pool(svc, id, kernel.Get<TelemetryClient>(), mediatorBuilder.Build())
                    );
            }).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}