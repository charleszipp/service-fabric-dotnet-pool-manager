using Microsoft.ServiceFabric.Services.Client;
using System.Fabric.Description;

namespace PoolManager.SDK
{
    public static class Extensions
    {
        public static System.Fabric.Description.PartitionSchemeDescription ToServiceFabricDescription(this PartitionSchemeDescription desc)
        {
            switch (desc)
            {
                case PartitionSchemeDescription.UniformInt64Name:
                    return new UniformInt64RangePartitionSchemeDescription();

                case PartitionSchemeDescription.Named:
                    return new NamedPartitionSchemeDescription();

                default:
                    return new SingletonPartitionSchemeDescription();
            }
        }

        public static ServicePartitionKey ToServicePartitionKey(this PartitionSchemeDescription desc, string instanceId)
        {
            switch (desc)
            {
                case SDK.PartitionSchemeDescription.UniformInt64Name:
                    return new ServicePartitionKey(1);
                case SDK.PartitionSchemeDescription.Named:
                    return new ServicePartitionKey(instanceId);
                default:
                    return null;
            }
        }
    }
}