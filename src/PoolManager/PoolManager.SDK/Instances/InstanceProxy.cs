﻿using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Instances.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.SDK.Instances
{
    public class InstanceProxy : IInstanceProxy
    {
        private readonly IActorProxyFactory _actorProxyFactory;

        public InstanceProxy(IActorProxyFactory actorProxyFactory)
        {
            _actorProxyFactory = actorProxyFactory;
        }

        public async Task<Guid> StartAsync(StartInstanceRequest request)
        {
            var rvalue = Guid.NewGuid();
            await GetProxy(rvalue).StartAsync(request);
            return rvalue;
        }

        public async Task<Guid> StartAsAsync(StartInstanceAsRequest request)
        {
            var rvalue = Guid.NewGuid();
            await GetProxy(rvalue).StartAsAsync(request);
            return rvalue;
        }

        public Task OccupyAsync(Guid instanceId, OccupyRequest request) => 
            GetProxy(instanceId).OccupyAsync(request);

        public Task<TimeSpan> ReportActivityAsync(Guid instanceId, ReportActivityRequest request) =>
            GetProxy(instanceId).ReportActivityAsync(request);

        public async Task RemoveAsync(Guid instanceId)
        {
            await GetProxy(instanceId).RemoveAsync();
            await GetServiceProxy(instanceId).DeleteActorAsync(new ActorId(instanceId), CancellationToken.None);
        }

        public Task VacateAsync(Guid instanceId) => 
            GetProxy(instanceId).VacateAsync();

        private IInstance GetProxy(Guid instanceId) =>
            _actorProxyFactory.CreateActorProxy<IInstance>(new ActorId(instanceId), "PoolManager", "InstanceActorService");

        internal IActorService GetServiceProxy(Guid instanceId) =>
            _actorProxyFactory.CreateActorServiceProxy<IActorService>(new Uri("fabric:/PoolManager/InstanceActorService"), new ActorId(instanceId));
    }
}
