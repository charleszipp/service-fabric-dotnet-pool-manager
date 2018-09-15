using System;
using System.Threading.Tasks;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;

namespace PoolManager.Instances
{
    public class InstanceStateOccupied : InstanceState
    {
        public override InstanceStates State => InstanceStates.Occupied;

        public override Task<InstanceState> OccupyAsync(InstanceContext context, OccupyRequest request) =>
            Task.FromResult<InstanceState>(this);

        public override async Task<InstanceState> RemoveAsync(InstanceContext context)
        {
            var rvalue = await VacateAsync(context);
            rvalue = await rvalue.RemoveAsync(context);
            return rvalue;
        }

        public override async Task<TimeSpan> ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            var state = await context.GetServiceStateAsync();
            state.LastActiveUtc = request.LastActiveUtc;
            await context.SetServiceStateAsync(state);

            var config = await context.GetInstanceConfigurationAsync();

            return TimeSpan.FromMilliseconds(config.ExpirationQuanta.TotalMilliseconds / 3);
        }

        public override Task<InstanceState> StartAsAsync(InstanceContext context, StartInstanceAsRequest request) =>
            throw new Exception("Invalid state transition. Instance is already started");

        public override Task<InstanceState> StartAsync(InstanceContext context, StartInstanceRequest request) =>
            throw new Exception("Invalid state transition. Instance is already started");

        public override async Task<InstanceState> VacateAsync(InstanceContext context)
        {
            var svc = await context.GetServiceInstanceProxy();
            await svc.VacateAsync();
            await context.SetServiceStateAsync(new ServiceState());
            return context.InstanceStates.Get(InstanceStates.Vacant);
        }
    }
}
