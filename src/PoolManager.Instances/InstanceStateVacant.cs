using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Fabric.Description;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Instances;

namespace PoolManager.Instances
{
    public class InstanceStateVacant : InstanceState
    {
        public override InstanceStates State => InstanceStates.Vacant;

        public override async Task<InstanceState> OccupyAsync(InstanceContext context, OccupyRequest request)
        {
            var svc = await context.GetServiceInstanceProxy();
            await svc.OccupyAsync(context.InstanceId, request.ServiceInstanceName);
            await context.SetServiceStateAsync(new ServiceState(request.ServiceInstanceName));
            return context.InstanceStates.Get(InstanceStates.Occupied);
        }

        public override Task<TimeSpan> ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to a vacant instance");
            return Task.FromResult(TimeSpan.MaxValue);
        }

        public override async Task<InstanceState> RemoveAsync(InstanceContext context)
        {
            var config = await context.GetInstanceConfigurationAsync();
            await context.Cluster.DeleteServiceAsync(config.ServiceInstanceUri);
            return context.InstanceStates.Get(InstanceStates.Idle);
        }

        public override Task<InstanceState> StartAsAsync(InstanceContext context, StartInstanceAsRequest request) => 
            throw new Exception($"Invalid state transition. Instance is already started");

        public override Task<InstanceState> StartAsync(InstanceContext context, StartInstanceRequest request) => 
            throw new Exception($"Invalid state transition. Instance is already started");

        public override Task<InstanceState> VacateAsync(InstanceContext context) => 
            Task.FromResult<InstanceState>(this);
    }
}
