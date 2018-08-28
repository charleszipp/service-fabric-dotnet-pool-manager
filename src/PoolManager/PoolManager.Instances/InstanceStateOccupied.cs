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

        public override async Task ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            var state = await context.StateManager.GetStateAsync<ServiceState>("service-state");
            state.LastActiveUtc = request.LastActiveUtc;
            await context.StateManager.SetStateAsync("service-state", state);
        }

        public override Task<InstanceState> StartAsAsync(InstanceContext context, StartAsRequest request) =>
            throw new Exception($"Invalid state transition. Instance is already started");

        public override Task<InstanceState> StartAsync(InstanceContext context, StartRequest request) =>
            throw new Exception($"Invalid state transition. Instance is already started");

        public override async Task<InstanceState> VacateAsync(InstanceContext context)
        {
            var svc = await context.GetServiceInstanceProxy();
            await svc.VacateAsync();
            await context.StateManager.SetStateAsync("service-state", new ServiceState());
            return context.InstanceStates.Get(InstanceStates.Vacant);
        }
    }
}
