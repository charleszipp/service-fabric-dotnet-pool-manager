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
            await context.StateManager.SetStateAsync("service-state", new ServiceState(request.ServiceInstanceName));
            return context.InstanceStates.Get(InstanceStates.Occupied);
        }

        public override Task ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to a vacant instance");
            return Task.CompletedTask;
        }

        public override async Task<InstanceState> RemoveAsync(InstanceContext context)
        {
            var config = await context.GetInstanceConfigurationAsync();
            DeleteServiceDescription ds = new DeleteServiceDescription(config.ServiceInstanceUri);
            Dictionary<string, string> properties = new Dictionary<string, string>()
                {
                    { "ServiceName", ds.ServiceName?.ToString() },
                    { "ForceDelete", ds.ForceDelete.ToString() }
                };
            try
            {
                await context.FabricClient.ServiceManager.DeleteServiceAsync(ds);
            }
            catch (TimeoutException ex)
            {
                //for timeout exceptions, record that they happened as traces but, 
                properties.Add("ExceptionMessage", ex.Message);
                properties.Add("ExceptionStack", ex.StackTrace);
                context.TelemetryClient.TrackTrace("Remove instance timed out", properties);
            }

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
