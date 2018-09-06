using System.Threading;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
using Ninject;
using PoolManager.Core;

namespace PoolManager.Instances
{
    internal static class Program
    {
        private static void Main()
        {
            var telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
            ActorRuntime.RegisterActorAsync<Instance>(
                (context, actorType) =>
                {
                    var kernel = new StandardKernel();
                    kernel.Bind<PoolsActorService>().ToMethod(x => new PoolsActorService(context, actorType, "InstanceActorServiceEndpoint",
                        (svc, id) => new Instance(svc, id, new ClusterClient(new System.Fabric.FabricClient(), telemetryClient),
                            telemetryClient,
                            new CorrelatingActorProxyFactory(context,
                                callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)),
                            new CorrelatingServiceProxyFactory(context,
                                callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)))));
                    return kernel.Get<PoolsActorService>();
                }).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}