using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.Domains.Instances;
using System;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class PartitionProxy : IPartitionProxy
    {
        private readonly IActorProxyFactory actors;

        public PartitionProxy(IActorProxyFactory actors)
        {
            this.actors = actors;
        }

        public Task VacateInstanceAsync(string partitionId, Guid instanceId, string instanceName)
        {
            throw new NotImplementedException();
        }
    }
}