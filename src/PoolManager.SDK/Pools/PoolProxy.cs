using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Pools.Requests;
using System.Threading.Tasks;
using PoolManager.SDK.Pools.Responses;

namespace PoolManager.SDK.Pools
{
    public class PoolProxy : IPoolProxy
    {
        private readonly IActorProxyFactory _actorProxyFactory;
        public PoolProxy(IActorProxyFactory actorProxyFactory) =>
            _actorProxyFactory = actorProxyFactory;
        public Task<ConfigurationResponse> GetConfigurationAsync(string serviceTypeUri) =>
            GetProxy(serviceTypeUri).GetConfigurationAsync();
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