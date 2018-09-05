using System;
using System.Fabric.Description;
using System.Threading.Tasks;

namespace PoolManager.Core
{
    public interface IClusterClient
    {
        Task CreateStatefulServiceAsync(string instanceId, string serviceTypeUri, PartitionSchemeDescription partitionSchemeDescription, int minReplicas = 1, int targetReplicas = 3, bool hasPersistedState = true);
        Task CreateStatefulServiceAsync(ServiceDescriptionFactory serviceDescriptionFactory, int minReplicas = 1, int targetReplicas = 3, bool hasPersistedState = true);
        Task CreateStatelessServiceAsync(string instanceId, string serviceTypeUri, PartitionSchemeDescription partitionSchemeDescription, int instanceCount = 1, byte[] initializationData = null);
        Task CreateStatelessServiceAsync(ServiceDescriptionFactory serviceDescriptionFactory, int instanceCount = 1, byte[] initializationData = null);
        Task DeleteServiceAsync(Uri serviceInstanceUri, bool force = false);
    }
}