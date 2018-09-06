using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using PoolManager.Core;
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
                kernel.Bind<TelemetryClient>().ToSelf();
                kernel.Bind<PoolsActorService>().ToMethod(x => new PoolsActorService(context, actorType, "PoolActorServiceEndpoint",
                    (svc, id) => new Pool(svc, id, kernel.Get<TelemetryClient>(), new InstanceProxy(new CorrelatingActorProxyFactory(context,
                            callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)), new GuidGetter()))));
                return kernel.Get<PoolsActorService>();
            }).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}