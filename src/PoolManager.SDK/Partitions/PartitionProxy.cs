using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.SDK.Partitions
{
    public class PartitionProxy : IPartitionProxy
    {
        private static readonly Uri ApplicationUri = new Uri("fabric:/PoolManager");
        private static readonly Uri ActorServiceUri = new Uri("fabric:/PoolManager/PartitionActorService");

        private readonly IActorProxyFactory _actorProxyFactory;
        private readonly FabricClient _fabricClient;

        public PartitionProxy(IActorProxyFactory actorProxyFactory, FabricClient fabricClient)
        {
            _actorProxyFactory = actorProxyFactory;
            _fabricClient = fabricClient;
        }

        public Task<GetInstanceResponse> GetInstanceAsync(string partitionId, GetInstanceRequest request) =>
            GetProxy(partitionId).GetInstanceAsync(request);

        public Task VacateInstanceAsync(string partitionId, VacateInstanceRequest request) =>
            GetProxy(partitionId).VacateInstanceAsync(request);

        public Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(string partitionId, string serviceTypeUri) => 
            GetProxy(partitionId).GetOccupiedInstancesAsync(new GetOccupiedInstancesRequest(serviceTypeUri));

        public async Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(string serviceTypeUri, CancellationToken cancellationToken)
        {
            var occupiedInstances = (await Task.WhenAll((await GetPartitionActorsAsync(cancellationToken))
                .Select(actor => GetOccupiedInstancesAsync(actor.ActorId.GetStringId(), serviceTypeUri))))
                .SelectMany(response => response.OccupiedInstances)
                .Distinct()
                .ToList();

            return new GetOccupiedInstancesResponse(occupiedInstances);
        }            

        private IPartition GetProxy(string partitionId) =>
            _actorProxyFactory.CreateActorProxy<IPartition>(new ActorId(partitionId), "PoolManager", "PartitionActorService");

        private async Task<IEnumerable<ActorInformation>> GetPartitionActorsAsync(CancellationToken cancellationToken) =>
            (await Task.WhenAll((await _fabricClient.GetInt64RangePartitionsAsync(ApplicationUri, ActorServiceUri)).Select(
                        partition => GetPartitionActorsAsync(partition.LowKey, cancellationToken))))
                .SelectMany(x => x).ToList();

        private async Task<IEnumerable<ActorInformation>> GetPartitionActorsAsync(long lowKey, CancellationToken cancellationToken) =>
            (await _actorProxyFactory.CreateActorServiceProxy<IActorService>(ActorServiceUri, lowKey + 1).GetActorsAsync(null, cancellationToken)).Items;
    }
}