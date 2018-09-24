using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using PoolManager.Core;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Pools;
using System.Fabric;
using System.Threading;

namespace PoolManager.Partitions
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Partition>((context, actorType) =>
            {
                var kernel = new StandardKernel();
                kernel.Bind<TelemetryClient>().ToSelf();
                kernel.Bind<PoolsActorService>().ToMethod(x => 
                    new PoolsActorService(context, actorType, "PoolActorServiceEndpoint",
                        (svc, id) => 
                        {
                            var actorProxyFactory = new CorrelatingActorProxyFactory(context, callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient));
                            var fabricClient = new FabricClient();
                            return new Partition(
                                svc,
                                id,
                                kernel.Get<TelemetryClient>(),
                                new InstanceProxy(
                                    actorProxyFactory,
                                    new GuidGetter()
                                ),
                                new PoolProxy(
                                    actorProxyFactory,
                                    new PartitionProxy(actorProxyFactory, fabricClient),
                                    fabricClient
                                )
                            );
                        }
                    )
                );
                return kernel.Get<PoolsActorService>();
            }).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}