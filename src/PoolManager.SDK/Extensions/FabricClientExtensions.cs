using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace PoolManager.SDK
{
    public static class FabricClientExtensions
    {
        public static async Task<IEnumerable<Int64RangePartitionInformation>> GetInt64RangePartitionsAsync(this FabricClient fabricClient, Uri appUri, Uri serviceUri)
        {
            var service = (await fabricClient.QueryManager.GetServiceListAsync(appUri)).FirstOrDefault(x => x.ServiceName.Equals(serviceUri));

            return service == null
                ? new List<Int64RangePartitionInformation>()
                : (await fabricClient.QueryManager.GetPartitionListAsync(service.ServiceName))
                .Where(x => x.PartitionInformation.Kind == ServicePartitionKind.Int64Range)
                .ToList()
                .ConvertAll(x => (Int64RangePartitionInformation)x.PartitionInformation);
        }
    }
}
