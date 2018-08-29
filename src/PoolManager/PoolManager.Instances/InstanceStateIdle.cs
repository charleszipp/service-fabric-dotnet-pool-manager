using System;
using System.Threading.Tasks;
using System.Fabric.Description;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Instances;

namespace PoolManager.Instances
{
    public class InstanceStateIdle : InstanceState
    {
        public override InstanceStates State => InstanceStates.Idle;

        public override Task<InstanceState> OccupyAsync(InstanceContext context, OccupyRequest request) => 
            throw new Exception($"Invalid state transition. Cannot occupy an idle service.");

        public override Task<InstanceState> RemoveAsync(InstanceContext context) => 
            Task.FromResult<InstanceState>(this);

        public override Task ReportActivityAsync(InstanceContext context, ReportActivityRequest request)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to an idle instance");
            return Task.CompletedTask;
        }

        public override async Task<InstanceState> StartAsAsync(InstanceContext context, StartInstanceAsRequest request)
        {
            var rvalue = await StartAsync(context, new StartInstanceRequest(request.ServiceTypeUri, request.IsServiceStateful, request.HasPersistedState, request.MinReplicas, request.TargetReplicas, request.PartitionScheme));
            rvalue = await rvalue.OccupyAsync(context, new OccupyRequest(request.ServiceInstanceName, request.ExpirationQuanta));
            return rvalue;
        }

        public override async Task<InstanceState> StartAsync(InstanceContext context, StartInstanceRequest request)
        {
            string applicationName = null;
            string serviceTypeName = null;
            context.ParseServiceTypeUri(request.ServiceTypeUri, out applicationName, out serviceTypeName);
            ServiceDescription serviceDescription = null;
            var instanceId = context.InstanceId;
            var instanceUri = context.CreateServiceInstanceUri(request.ServiceTypeUri, instanceId);
            var config = new ServiceConfiguration(instanceUri, request.IsServiceStateful, request.HasPersistedState, request.MinReplicas, request.TargetReplicas, request.PartitionScheme);

            if (config.IsServiceStateful)
            {
                serviceDescription = new StatefulServiceDescription()
                {
                    ApplicationName = new Uri(applicationName, UriKind.RelativeOrAbsolute),
                    MinReplicaSetSize = config.MinReplicas,
                    TargetReplicaSetSize = config.TargetReplicas,
                    HasPersistedState = config.HasPersistedState,
                    InitializationData = null,
                    ServiceTypeName = $"{serviceTypeName}",
                    ServiceName = config.ServiceInstanceUri
                };
            }
            else
            {
                serviceDescription = new StatelessServiceDescription()
                {
                    ApplicationName = new Uri(applicationName, UriKind.RelativeOrAbsolute),
                    InstanceCount = 1,
                    InitializationData = null,
                    ServiceTypeName = $"{serviceTypeName}",
                    ServiceName = config.ServiceInstanceUri
                };
            }
            switch (request.PartitionScheme)
            {
                case SDK.PartitionSchemeDescription.Singleton:
                    serviceDescription.PartitionSchemeDescription = new SingletonPartitionSchemeDescription();
                    break;
                case SDK.PartitionSchemeDescription.UniformInt64Name:
                    serviceDescription.PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription();
                    break;
                case SDK.PartitionSchemeDescription.Named:
                    serviceDescription.PartitionSchemeDescription = new NamedPartitionSchemeDescription();
                    break;
            }

            await context.FabricClient.ServiceManager.CreateServiceAsync(serviceDescription);
            await context.SetInstanceConfigurationAsync(config);

            return context.InstanceStates.Get(InstanceStates.Vacant);
        }

        public override Task<InstanceState> VacateAsync(InstanceContext context)
        {
            throw new Exception($"Invalid state transition. Cannot vacate an idle service.");
        }
    }
}
