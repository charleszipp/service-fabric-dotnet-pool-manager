using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.Core;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using PoolManager.SDK;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class ServiceInstanceProxy : IServiceInstanceProxy
    {
        private readonly IServiceProxyFactory services;
        private readonly IClusterClient cluster;

        public ServiceInstanceProxy(IServiceProxyFactory services, IClusterClient cluster)
        {
            this.services = services;
            this.cluster = cluster;
        }

        public Task DeleteAsync(Uri serviceUri, CancellationToken cancellationToken) => 
            cluster.DeleteServiceAsync(serviceUri);

        public Task OccupyAsync(Uri serviceUri, Guid instanceId, string instanceName) =>
            services.CreateServiceProxy<IServiceInstance>(serviceUri).OccupyAsync(instanceId.ToString(), instanceName);

        public Task VacateAsync(Uri serviceUri) => 
            services.CreateServiceProxy<IServiceInstance>(serviceUri).VacateAsync();
    }
}