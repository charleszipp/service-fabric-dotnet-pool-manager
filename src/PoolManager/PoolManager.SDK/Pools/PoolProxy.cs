using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.SDK.Pools
{
    public class PoolProxy : IPoolProxy
    {
        private readonly IActorProxyFactory _actorProxyFactory;
        public PoolProxy(IActorProxyFactory actorProxyFactory) =>
            _actorProxyFactory = actorProxyFactory;
        public Task DeletePoolAsync(string serviceTypeUri) =>
            GetServiceProxy(serviceTypeUri).DeleteActorAsync(new ActorId(serviceTypeUri), CancellationToken.None);
        public Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri) =>
            GetProxy(serviceTypeUri).GetConfigurationAsync();
        public Task GetInstanceAsync(string serviceTypeUri, GetInstanceRequest request) =>
            GetProxy(serviceTypeUri).GetAsync(request);
        public async Task StartPoolAsync(string serviceTypeUri, StartPoolRequest request) =>
            await GetProxy(serviceTypeUri).StartAsync(request);
        public async Task VacateInstanceAsync(string serviceTypeUri, VacateInstanceRequest request) =>
            await GetProxy(serviceTypeUri).VacateInstanceAsync(request);
        internal IActorService GetServiceProxy(string serviceTypeUri) =>
            _actorProxyFactory.CreateActorServiceProxy<IActorService>(new Uri("fabric:/PoolManager/PoolActorService"),
                new ActorId(serviceTypeUri));
        private IPool GetProxy(string serviceTypeUri) =>
            _actorProxyFactory.CreateActorProxy<IPool>(new ActorId(serviceTypeUri), "PoolManager", "PoolActorService");
    }
}