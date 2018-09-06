using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Threading.Tasks;

namespace PoolManager.Core
{
    public class ClusterClient : IClusterClient
    {
        private readonly FabricClient _fabricClient;
        private readonly TelemetryClient _telemetryClient;

        public ClusterClient(FabricClient fabricClient, TelemetryClient telemetryClient)
        {
            _fabricClient = fabricClient;
            _telemetryClient = telemetryClient;
        }

        public Task CreateStatelessServiceAsync(string instanceId, string serviceTypeUri, PartitionSchemeDescription partitionSchemeDescription, int instanceCount = 1, byte[] initializationData = null)
        {
            var serviceDescriptionFactory = new ServiceDescriptionFactory(serviceTypeUri, instanceId, partitionSchemeDescription);
            return CreateStatelessServiceAsync(serviceDescriptionFactory, instanceCount, initializationData);            
        }

        public Task CreateStatelessServiceAsync(ServiceDescriptionFactory serviceDescriptionFactory, int instanceCount = 1, byte[] initializationData = null)
        {
            var serviceDescription = serviceDescriptionFactory.CreateStateless(instanceCount, initializationData);
            return _fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
        }

        public Task CreateStatefulServiceAsync(string instanceId, string serviceTypeUri, PartitionSchemeDescription partitionSchemeDescription, int minReplicas = 1, int targetReplicas = 3, bool hasPersistedState = true)
        {
            var serviceDescriptionFactory = new ServiceDescriptionFactory(serviceTypeUri, instanceId, partitionSchemeDescription);
            return CreateStatefulServiceAsync(serviceDescriptionFactory, minReplicas, targetReplicas, hasPersistedState);
        }

        public Task CreateStatefulServiceAsync(ServiceDescriptionFactory serviceDescriptionFactory, int minReplicas = 1, int targetReplicas = 3, bool hasPersistedState = true)
        {
            var serviceDescription = serviceDescriptionFactory.CreateStateful(minReplicas, targetReplicas, hasPersistedState);
            return _fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
        }

        public async Task DeleteServiceAsync(Uri serviceInstanceUri, bool force = false)
        {
            var ds = new DeleteServiceDescription(serviceInstanceUri) {ForceDelete = force};
            try { await _fabricClient.ServiceManager.DeleteServiceAsync(ds); }
            catch (TimeoutException ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"ServiceName", ds.ServiceName?.ToString()}, {"ForceDelete", ds.ForceDelete.ToString()},
                    {"ExceptionMessage", ex.Message}, {"ExceptionStack", ex.StackTrace}
                };
                _telemetryClient.TrackTrace("Remove instance timed out", properties);
            }
        }
    }
}
