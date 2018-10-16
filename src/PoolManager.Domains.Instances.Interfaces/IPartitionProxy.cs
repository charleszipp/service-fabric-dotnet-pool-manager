using System;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances.Interfaces
{
    public interface IPartitionProxy
    {
        Task VacateInstanceAsync(string partitionId, string serviceTypeUri, string instanceName, Guid instanceId);
    }
}
