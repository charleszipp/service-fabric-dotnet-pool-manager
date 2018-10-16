using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using System.Threading;

namespace PoolManager.Instances
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Instance>(
                (context, actorType) =>
                    new PoolsActorService(
                        context,
                        actorType,
                        "InstanceActorServiceEndpoint",
                        (svc, id) => new Instance(svc, id))
                ).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}