using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.SDK.Pools;

namespace PoolManager.Pools
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Pool : Actor, IPool
    {
        public Pool(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
    }
}