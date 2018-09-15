using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Partitions.Interfaces;
using PoolManager.Partitions.Models;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System;
using System.Threading.Tasks;

namespace PoolManager.Partitions
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Partitions : Actor, IPartitions
    {
        public Partitions(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request)
        {
            var instance = await StateManager.TryGetStateAsync<MappedInstance>(GetStateName(request.ServiceTypeUri, request.InstanceName));
            if (instance.HasValue)
                return new GetInstanceResponse(instance.Value.ServiceName);
            else
            {
                //todo: ask pool for a vacant instance
                //todo: occupy vacant instance from pool
                //todo: return service name of occupied instance
                //todo: if occupy fails or takes over a certain time, mark the instance for deletion and retry
                //todo: add the occupied instance back to the state manager

                throw new ArgumentException("Unable to find a mapped instance for given pool and name");
            }
        }

        private string GetStateName(string serviceTypeUri, string serviceInstanceName)
        {
            return $"{serviceTypeUri.TrimEnd('/')}/{serviceInstanceName}";
        }
    }
}