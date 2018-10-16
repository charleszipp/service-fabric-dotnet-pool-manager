using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Core;
using System.Threading;

namespace PoolManager.Partitions
{
    internal static class Program
    {
        private static void Main()
        {
            ActorRuntime.RegisterActorAsync<Partition>(
                (context, actorType) =>
                    new PoolsActorService(
                        context,
                        actorType,
                        "PartitionActorServiceEndpoint",
                        (svc, id) => new Partition(svc, id))
                ).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}