using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using System.Threading;

namespace PoolManager.Pools
{
    internal static class Program
    {
        private static void Main()
        {
            var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
            ActorRuntime.RegisterActorAsync<Pool>(
                   (context, actorType) =>
                    new PoolsActorService(
                        context,
                        actorType,
                        "PoolActorServiceEndpoint",
                        (svc, id) => new Pool(svc, id, telemetryClient)
                    )
            ).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}