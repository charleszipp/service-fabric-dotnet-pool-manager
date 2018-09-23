using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.SDK
{
    public static class FabricClientExtensions
    {
        public static async Task<IEnumerable<Int64RangePartitionInformation>> GetInt64RangePartitionsAsync(this FabricClient fabricClient, Uri appUri, Uri serviceUri)
        {
            var service = (await fabricClient.QueryManager.GetServiceListAsync(appUri)).FirstOrDefault(x => x.ServiceName.Equals(serviceUri));

            return service == null
                ? new List<Int64RangePartitionInformation>()
                : (await fabricClient.QueryManager.GetPartitionListAsync(service.ServiceName))
                .Where(x => x.PartitionInformation.Kind == ServicePartitionKind.Int64Range)
                .ToList()
                .ConvertAll(x => (Int64RangePartitionInformation)x.PartitionInformation);
        }

        public static async Task<IEnumerable<ActorInformation>> GetActorsAsync(this FabricClient fabricClient, IActorProxyFactory actorProxyFactory, Uri applicationUri, Uri actorServiceUri, CancellationToken cancellationToken) =>
            (await Task.WhenAll((await fabricClient.GetInt64RangePartitionsAsync(applicationUri, actorServiceUri)).Select(
                        partition => fabricClient.GetActorsAsync(actorProxyFactory, actorServiceUri, partition.LowKey, cancellationToken))))
                .SelectMany(x => x).ToList();

        public static async Task<IEnumerable<ActorInformation>> GetActorsAsync(this FabricClient fabricClient, IActorProxyFactory actorProxyFactory, Uri actorServiceUri, long lowKey, CancellationToken cancellationToken) =>
            (await actorProxyFactory.CreateActorServiceProxy<IActorService>(actorServiceUri, lowKey + 1).GetActorsAsync(null, cancellationToken)).Items;
    }
}
