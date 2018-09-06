using System;
using System.Fabric.Description;

namespace PoolManager.Core
{
    public class ServiceDescriptionFactory
    {
        public ServiceDescriptionFactory(string serviceTypeUri, string instanceId, PartitionSchemeDescription partitionSchemeDescription)
        {
            ParseServiceTypeUri(serviceTypeUri, out var applicationName, out var serviceTypeName);
            ApplicationName = new Uri(applicationName, UriKind.RelativeOrAbsolute);
            ServiceTypeName = serviceTypeName;
            ServiceName = CreateServiceName(serviceTypeUri, instanceId);
            PartitionSchemeDescription = partitionSchemeDescription;
        }

        public Uri ApplicationName { get; }

        public string ServiceTypeName { get; }

        public Uri ServiceName { get; }

        public PartitionSchemeDescription PartitionSchemeDescription { get; }

        public static void ParseServiceTypeUri(string serviceTypeUri, out string applicationName, out string serviceTypeName)
        {
            var indexLastSlash = serviceTypeUri.LastIndexOf('/');
            if (indexLastSlash >= 0)
            {
                applicationName = serviceTypeUri.Substring(0, indexLastSlash);
                serviceTypeName = serviceTypeUri.Substring(indexLastSlash + 1);
            }
            else
            {
                applicationName = null;
                serviceTypeName = null;
            }
        }

        public static Uri CreateServiceName(string serviceTypeUri, string instanceId)
        {
            return new Uri($"{serviceTypeUri}/{instanceId}", UriKind.RelativeOrAbsolute);
        }

        public StatelessServiceDescription CreateStateless(int instanceCount = 1, byte[] initializationData = null)
        {
            return new StatelessServiceDescription
            {
                ApplicationName = ApplicationName,
                InstanceCount = instanceCount,
                InitializationData = initializationData,
                ServiceTypeName = ServiceTypeName,
                ServiceName = ServiceName,
                PartitionSchemeDescription = PartitionSchemeDescription
            };
        }

        public StatefulServiceDescription CreateStateful(int minReplicas = 1, int targetReplicas = 3, bool hasPersistedState = true)
        {
            return new StatefulServiceDescription
            {
                ApplicationName = ApplicationName,
                MinReplicaSetSize = minReplicas,
                TargetReplicaSetSize = targetReplicas,
                HasPersistedState = hasPersistedState,
                InitializationData = null,
                ServiceTypeName = ServiceTypeName,
                ServiceName = ServiceName,
                PartitionSchemeDescription = PartitionSchemeDescription
            };
        }
    }
}