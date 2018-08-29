using System.Threading.Tasks;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;

namespace PoolManager.Instances
{
    public abstract class InstanceState
    {
        public abstract InstanceStates State { get; }
        public abstract Task<InstanceState> StartAsync(InstanceContext context, StartInstanceRequest request);
        public abstract Task<InstanceState> StartAsAsync(InstanceContext context, StartInstanceAsRequest request);
        public abstract Task<InstanceState> OccupyAsync(InstanceContext context, OccupyRequest request);
        public abstract Task<InstanceState> VacateAsync(InstanceContext context);
        public abstract Task<InstanceState> RemoveAsync(InstanceContext context);
        public abstract Task ReportActivityAsync(InstanceContext context, ReportActivityRequest request);
    }
}
