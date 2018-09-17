using System.Fabric;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
using Ninject;
using PoolManager.Core;
using PoolManager.Core.Mediators;
using PoolManager.Core.Mediators.Builders;
using PoolManager.Core.Mediators.Resolvers;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.States;

namespace PoolManager.Instances
{
    internal static class Program
    {
        private static void Main()
        {
            var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
            ActorRuntime.RegisterActorAsync<Instance>(
                (context, actorType) =>
                {
                    var kernel = new StandardKernel();
                    kernel.Bind<ServiceContext>().ToConstant(context);
                    kernel.Bind<TelemetryClient>().ToSelf();
                    kernel.Bind<IActorProxyFactory>().ToMethod(ctx =>
                        new CorrelatingActorProxyFactory(
                            ctx.Kernel.Get<ServiceContext>(),
                            callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                            )
                        ).InSingletonScope();
                    kernel.Bind<IServiceProxyFactory>().ToMethod(ctx =>
                        new CorrelatingServiceProxyFactory(
                            ctx.Kernel.Get<ServiceContext>(),
                            callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                            )
                        ).InSingletonScope();

                    kernel.Bind<IInstanceRepository>().To<InstanceRepository>();
                    kernel.Bind<IServiceInstanceProxy>().To<ServiceInstanceProxy>();
                    kernel.Bind<IPartitionProxy>().To<PartitionProxy>();
                    kernel.Bind<IInstanceStateProvider>().To<InstanceStateProvider>();
                    kernel.Bind<Mediator>().ToMethod(ctx =>
                        new MediatorBuilder(new NinjectDependencyResolver(kernel))
                        .WithInstances()
                        .Build())
                        .InSingletonScope();
                    kernel.Bind<InstanceContext>().ToSelf();
                    return new PoolsActorService(
                        context,
                        actorType,
                        "InstanceActorServiceEndpoint",
                        (svc, id) =>
                            new Instance(svc, id, 
                                kernel.Get<TelemetryClient>(), 
                                kernel.Get<InstanceContext>(), 
                                kernel.Get<IInstanceRepository>()
                            )
                        );
                }).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}