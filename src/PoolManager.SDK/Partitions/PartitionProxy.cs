using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System.Threading.Tasks;

namespace PoolManager.SDK.Partitions
{
    public class PartitionProxy : IPartitionProxy
    {
        private readonly IActorProxyFactory _actorProxyFactory;

        public PartitionProxy(IActorProxyFactory actorProxyFactory) =>
            _actorProxyFactory = actorProxyFactory;

        public Task<GetInstanceResponse> GetInstanceAsync(string partitionId, GetInstanceRequest request) =>
            GetProxy(partitionId).GetInstanceAsync(request);

        private IPartition GetProxy(string partitionId) =>
            _actorProxyFactory.CreateActorProxy<IPartition>(new ActorId(partitionId), "PoolManager", "PartitionActorService");
    }
}