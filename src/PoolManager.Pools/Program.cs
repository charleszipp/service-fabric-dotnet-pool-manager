using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Ninject;

namespace PoolManager.Pools
{
    internal static class Program
    {
        private static void Main()
        {
            var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
            ActorRuntime.RegisterActorAsync<Pool>(
                   (context, actorType) =>
                   {
                       var kernel = new StandardKernel();
                       kernel.Bind<TelemetryClient>().ToSelf();
                       kernel.Bind<PoolsActorService>().ToMethod(x => new PoolsActorService(
                           context,
                           actorType,
                           "PoolActorServiceEndpoint",
                           (svc, id) => new Pool(
                                svc, 
                                id, 
                                kernel.Get<TelemetryClient>(), 
                                new CorrelatingActorProxyFactory(svc.Context, callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient))
                            )
                       ));
                       return kernel.Get<PoolsActorService>();
                   }).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}