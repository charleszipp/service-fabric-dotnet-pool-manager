using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using PoolManager.Core;
using System.Threading;

namespace PoolManager.Partitions
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Partitions>((context, actorType) =>
            {
                var kernel = new StandardKernel();
                kernel.Bind<TelemetryClient>().ToSelf();
                kernel.Bind<PoolsActorService>().ToMethod(x => new PoolsActorService(context, actorType, "PoolActorServiceEndpoint",
                    (svc, id) => new Partitions(svc, id)));
                return kernel.Get<PoolsActorService>();
            }).GetAwaiter().GetResult();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}