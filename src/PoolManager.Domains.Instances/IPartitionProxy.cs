using System;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public interface IPartitionProxy
    {
        Task VacateInstanceAsync(string partitionId, Guid instanceId, string instanceName);
    }
}
