using System;
using System.Threading.Tasks;
using PoolManager.SDK.Instances.Requests;

namespace PoolManager.SDK.Instances
{
    public interface IInstanceProxy
    {
        Task OccupyAsync(Guid instanceId, OccupyRequest request);
        Task RemoveAsync(Guid instanceId);
        Task<Guid> StartAsAsync(StartInstanceAsRequest request);
        Task<Guid> StartAsync(StartInstanceRequest request);
    }
}