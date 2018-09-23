using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;
using PoolManager.SDK.Partitions;
using System.Threading;
using System;
using System.Fabric;
using System.Linq;
using System.Collections.Generic;

namespace PoolManager.SDK.Pools
{
    public class PoolProxy : IPoolProxy
    {
        private static readonly Uri ApplicationUri = new Uri("fabric:/PoolManager");
        private static readonly Uri ActorServiceUri = new Uri("fabric:/PoolManager/PoolActorService");
        private readonly IActorProxyFactory _actorProxyFactory;
        private readonly IPartitionProxy _partitions;
        private readonly FabricClient _fabricClient;

        public PoolProxy(IActorProxyFactory actorProxyFactory, IPartitionProxy partitions, FabricClient fabricClient)
        {
            _actorProxyFactory = actorProxyFactory;
            _partitions = partitions;
            _fabricClient = fabricClient;
        }

        public Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri) =>
            GetProxy(serviceTypeUri).GetConfigurationAsync();

        public async Task<GetInstancesResponse> GetInstancesAsync(string serviceTypeUri, CancellationToken cancellationToken)
        {
            var occupiedInstancesResponse = await _partitions.GetOccupiedInstancesAsync(serviceTypeUri, cancellationToken);
            var vacantInstancesResponse = await GetProxy(serviceTypeUri).GetVacantInstancesAsync();
            return new GetInstancesResponse(serviceTypeUri, vacantInstancesResponse.VacantInstances, occupiedInstancesResponse.OccupiedInstances);
        }

        public async Task<IEnumerable<GetInstancesResponse>> GetInstancesAsync(CancellationToken cancellationToken) => 
            (await Task.WhenAll((await _fabricClient.GetActorsAsync(_actorProxyFactory, ApplicationUri, ActorServiceUri, cancellationToken))
                .Select(actor => GetInstancesAsync(actor.ActorId.GetStringId(), cancellationToken))))
                .ToList();

        public Task<GetVacantInstancesResponse> GetVacantInstancesAsync(string serviceTypeUri) => 
            GetProxy(serviceTypeUri).GetVacantInstancesAsync();
        public Task<PopVacantInstanceResponse> PopVacantInstanceAsync(string serviceTypeUri, PopVacantInstanceRequest request) =>
            GetProxy(serviceTypeUri).PopVacantInstanceAsync(request);
        public async Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request) =>
            await GetProxy(serviceTypeUri).StartAsync(request);

        private IPool GetProxy(string serviceTypeUri) =>
            _actorProxyFactory.CreateActorProxy<IPool>(new ActorId(serviceTypeUri), "PoolManager", "PoolActorService");
    }
}