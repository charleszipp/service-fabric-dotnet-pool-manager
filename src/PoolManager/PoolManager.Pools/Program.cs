using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using System.Threading;
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
                       kernel.Bind<PoolsActorService>().ToMethod(x => new PoolsActorService(
                           context,
                           actorType,
                           "PoolActorServiceEndpoint",
                           (svc, id) => new Pool(svc, id, telemetryClient)
                       ));

                       return kernel.Get<PoolsActorService>();
                   }).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}