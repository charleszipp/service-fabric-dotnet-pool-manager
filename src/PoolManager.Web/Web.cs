using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ApplicationInsights.ServiceFabric.Module;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1;
using Microsoft.ServiceFabric.Services.Remoting.V1.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Pools;

namespace PoolManager.Web
{
    internal sealed class Web : StatelessService
    {
        public Web(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "internal", (url, listener) =>
                    {
                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton(serviceContext)
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => new FabricTelemetryInitializer())
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
                                            .AddSingleton<ITelemetryModule>(new ServiceRemotingDependencyTrackingTelemetryModule())
                                            .AddSingleton<ITelemetryModule>(new ServiceRemotingRequestTrackingTelemetryModule())
                                            .AddSingleton<IActorProxyFactory>((sp) => new CorrelatingActorProxyFactory(serviceContext, callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)))
                                            .AddSingleton<FabricClient>()
                                            .AddTransient<IPartitionProxy, PartitionProxy>()
                                            .AddTransient<IPoolProxy, PoolProxy>()
                                        )
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseApplicationInsights()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseReverseProxyIntegration)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
