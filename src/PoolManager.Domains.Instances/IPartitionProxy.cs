using System;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public interface IPartitionProxy
    {
        Task VacateInstanceAsync(string partitionId, string serviceTypeUri, string instanceName, Guid instanceId);
    }
}
