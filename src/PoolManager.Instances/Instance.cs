using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    [StatePersistence(StatePersistence.Persisted)]
    public class Instance : Actor, IInstance, IRemindable
    {
        private readonly InstanceContext _context;
        private readonly TelemetryClient _telemetryClient;

        public Instance(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient)
            : base(actorService, actorId)
        {
            _context = new InstanceContext(
                GetInstanceId(),
                new InstanceStateProvider(new InstanceStateIdle(), new InstanceStateVacant(), new InstanceStateOccupied()),
                new PoolProxy(new CorrelatingActorProxyFactory(ActorService.Context, callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient))),
                new CorrelatingServiceProxyFactory(ActorService.Context, callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)),
                new FabricClient(),
                StateManager,
                telemetryClient
                );
            _telemetryClient = telemetryClient;
        }

        public Task StartAsync(StartInstanceRequest request) => _context.StartAsync(request);

        public Task StartAsAsync(StartInstanceAsRequest request) => _context.StartAsAsync(request);

        public async Task RemoveAsync()
        {
            try
            {
                await UnregisterReminderAsync(GetReminder("expiration-quanta"));
            }
            catch (ReminderNotFoundException) { }
            await _context.RemoveAsync();
        }

        public async Task OccupyAsync(OccupyRequest request)
        {
            await _context.OccupyAsync(request);
            //calculate a seed value to the nearest 100ms to stagger the due time of the different actors.
            //this prevents from all actors occupied within milliseconds of each other from all vacating at exactly the same time
            //which will cause a deadlock on the pool actor.
            var config = await _context.GetInstanceConfigurationAsync();
            var intervalMs = (int)Math.Round(config.ExpirationQuanta.TotalMilliseconds / 5);
            var dueMs = ((int)Math.Round((GetInstanceId().GetHashCode() % 1000) / 100.0) * 100) + intervalMs;
            await RegisterReminderAsync("expiration-quanta", null, TimeSpan.FromMilliseconds(dueMs), TimeSpan.FromMilliseconds(intervalMs));
        }

        public Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request) => _context.ReportActivityAsync(request);

        public async Task VacateAsync()
        {
            try
            {
                await UnregisterReminderAsync(GetReminder("expiration-quanta"));
            }
            catch (ReminderNotFoundException) { }
            await _context.VacateAsync();
        }

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

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            using (var request = _telemetryClient.StartOperation<RequestTelemetry>(reminderName))
            {
                try
                {
                    if (reminderName.Equals("expiration-quanta"))
                    {
                        var serviceState = await _context.GetServiceStateAsync();
                        var config = await _context.GetInstanceConfigurationAsync();
                        TimeSpan inactivityPeriod = DateTime.UtcNow.Subtract(serviceState.LastActiveUtc.Value);
                        if (inactivityPeriod > config.ExpirationQuanta)
                        {
                            var vacateInstanceRequest = new VacateInstanceRequest(this.GetActorId().GetGuidId(), serviceState.ServiceInstanceName);
                            await _context.PoolProxy.VacateInstanceAsync(config.ServiceTypeUri, vacateInstanceRequest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    request.Telemetry.Success = false;
                    _telemetryClient.TrackException(ex);
                }
            }
        }
    }
}