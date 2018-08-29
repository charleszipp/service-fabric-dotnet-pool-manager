using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.SDK.Pools
{
    public class PoolProxy : IPoolProxy
    {
        private readonly IActorProxyFactory _actorProxyFactory;

        public PoolProxy(IActorProxyFactory actorProxyFactory) => 
            _actorProxyFactory = actorProxyFactory;

        public async Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request) => 
            await GetProxy(serviceTypeUri).VacateInstanceAsync(request);

        private IPool GetProxy(string serviceTypeUri) =>
            _actorProxyFactory.CreateActorProxy<IPool>(new ActorId(serviceTypeUri), "PoolManager", "PoolActorService");
    }
}
