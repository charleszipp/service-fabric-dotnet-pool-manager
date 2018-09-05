using System;
using System.Threading.Tasks;
using System.Fabric.Description;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Instances;
using PoolManager.SDK;
using PoolManager.Core;

namespace PoolManager.Instances
{
    public class InstanceStateIdle : InstanceState
    {
        public override InstanceStates State => InstanceStates.Idle;

        public override Task<InstanceState> OccupyAsync(InstanceContext context, OccupyRequest request) => 
            throw new Exception($"Invalid state transition. Cannot occupy an idle service.");

        public override Task<InstanceState> RemoveAsync(InstanceContext context) => 
            Task.FromResult<InstanceState>(this);

        public override Task<TimeSpan> ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to an idle instance");
            return Task.FromResult(TimeSpan.MaxValue);
        }

        public override async Task<InstanceState> StartAsAsync(InstanceContext context, StartInstanceAsRequest request)
        {
            var rvalue = await StartAsync(context, new StartInstanceRequest(request.ServiceTypeUri, request.IsServiceStateful, request.HasPersistedState, request.MinReplicas, request.TargetReplicas, request.PartitionScheme));
            rvalue = await rvalue.OccupyAsync(context, new OccupyRequest(request.ServiceInstanceName, request.ExpirationQuanta));
            return rvalue;
        }

        public override async Task<InstanceState> StartAsync(InstanceContext context, StartInstanceRequest request)
        {
            var partitionSchemeDescription = request.PartitionScheme.ToServiceFabricDescription();
            var serviceDescriptionFactory = new ServiceDescriptionFactory(request.ServiceTypeUri, context.InstanceId, partitionSchemeDescription);
            var config = new ServiceConfiguration(serviceDescriptionFactory.ServiceName, request.ServiceTypeUri, request.IsServiceStateful, request.HasPersistedState, request.MinReplicas, request.TargetReplicas, request.PartitionScheme, request.ExpirationQuanta);

            if (config.IsServiceStateful)
                await context.Cluster.CreateStatefulServiceAsync(serviceDescriptionFactory, request.MinReplicas, request.TargetReplicas, request.HasPersistedState);
            else
                await context.Cluster.CreateStatelessServiceAsync(serviceDescriptionFactory);
            await context.SetInstanceConfigurationAsync(config);
            return context.InstanceStates.Get(InstanceStates.Vacant);
        }

        public override Task<InstanceState> VacateAsync(InstanceContext context)
        {
            throw new Exception($"Invalid state transition. Cannot vacate an idle service.");
        }
    }
}
