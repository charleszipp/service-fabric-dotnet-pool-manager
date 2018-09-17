using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.SDK.Partitions;
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;

namespace PoolManager.SDK
{
    public class PooledServiceProxy<TService> : RealProxy
        where TService : IService
    {
        private readonly TService _service;
        private readonly Random _random = new Random();

        protected PooledServiceProxy(TService service)
            : base(typeof(TService)) => _service = service;

        public static async Task<TService> Create(IActorProxyFactory actorProxyFactory, IServiceProxyFactory serviceProxyFactory, string partitionId, string serviceTypeUri, string serviceInstanceName, ServicePartitionKey servicePartitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
        {
            var partition = actorProxyFactory.CreateActorProxy<IPartition>(new ActorId(partitionId));
            var instance = await partition.GetInstanceAsync(new Partitions.Requests.GetInstanceRequest(serviceTypeUri, serviceInstanceName));
            var serviceProxy = serviceProxyFactory.CreateServiceProxy<TService>(instance.ServiceInstanceUri, servicePartitionKey, targetReplicaSelector, listenerName);
            return Create(serviceProxy);
        }

        public static TService Create(TService service) =>
            (TService)new PooledServiceProxy<TService>(service).GetTransparentProxy();

        public override IMessage Invoke(IMessage msg)
        {
            var task = OnInvokeAsync(msg);
            task.Wait();
            return task.Result;
        }

        protected virtual async Task<IMessage> OnInvokeAsync(IMessage msg, int previousAttempts = 0, int waitMs = 1000)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;
            var attempts = previousAttempts + 1;
            try
            {
                var result = method.Invoke(_service, methodCall.InArgs);

                if (result is Task)
                    await ((Task)result).ConfigureAwait(false);

                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (AggregateException ex)
            {
                if ((ex.InnerException is InstanceNotActivatedException || ex.InnerException?.InnerException is InstanceNotActivatedException) && attempts < 10)
                {
                    var randomSeed = _random.Next(0, 100);
                    int delay = (waitMs + randomSeed) * attempts;
                    if (delay > 10000)
                        delay = 10000;
                    await Task.Delay(delay).ConfigureAwait(false);
                    return await OnInvokeAsync(msg, attempts, waitMs).ConfigureAwait(false);
                }
                else
                    return new ReturnMessage(ex, methodCall);
            }
        }
    }
}