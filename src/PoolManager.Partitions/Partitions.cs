using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PoolManager.Partitions.Interfaces;

namespace PoolManager.Partitions
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Partitions : Actor, IPartitions
    {
        public Partitions(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
    }
}