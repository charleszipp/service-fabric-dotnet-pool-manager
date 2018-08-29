using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using System.Fabric;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Instance : Actor, IInstance
    {
        private readonly InstanceContext _context;

        public Instance(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient)
            : base(actorService, actorId)
        {
            _context = new InstanceContext(
                GetInstanceId(),
                new InstanceStateProvider(new InstanceStateIdle(), new InstanceStateVacant(), new InstanceStateOccupied()),
                new CorrelatingServiceProxyFactory(ActorService.Context, callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)),
                new FabricClient(),
                StateManager,
                telemetryClient
                );
        }

        public Task StartAsync(StartInstanceRequest request) => _context.StartAsync(request);

        public Task StartAsAsync(StartInstanceAsRequest request) => _context.StartAsAsync(request);

        public Task RemoveAsync() => _context.RemoveAsync();

        public Task OccupyAsync(OccupyRequest request) => _context.OccupyAsync(request);

        public Task ReportActivityAsync(ReportActivityRequest request) => _context.ReportActivityAsync(request);

        protected override Task OnActivateAsync() => _context.ActivateAsync();

        protected override Task OnDeactivateAsync() => _context.DeactivateAsync();        

        private string GetInstanceId()
        {
            string rvalue = null;

            var actorId = this.GetActorId();
            switch (actorId.Kind)
            {
                case ActorIdKind.Guid:
                    rvalue = actorId.GetGuidId().ToString();
                    break;

                case ActorIdKind.String:
                    rvalue = actorId.GetStringId();
                    break;

                case ActorIdKind.Long:
                    rvalue = actorId.GetLongId().ToString();
                    break;
            }

            return rvalue;
        }
    }
}