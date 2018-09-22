using System.Fabric;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;

namespace PoolManager.Pools
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Pool>((context, actorType) =>
            {
                return new PoolsActorService(
                    context,
                    actorType,
                    "PoolActorServiceEndpoint",
                    (svc, id) =>
                        new Pool(svc, id)
                    );
            }).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}