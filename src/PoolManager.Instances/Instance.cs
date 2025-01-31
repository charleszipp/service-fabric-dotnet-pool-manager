﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Ninject;
using PoolManager.Core;
using PoolManager.Core.Mediators;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.States;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Instances.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    [StatePersistence(StatePersistence.Persisted)]
    public class Instance : Actor, IInstance, IRemindable
    {
        private readonly InstanceContext _context;
        private readonly IInstanceRepository _repository;
        private readonly TelemetryClient _telemetryClient;
        private readonly CancellationToken _cancellation = default(CancellationToken);
        private readonly IKernel _kernel;

        public Instance(ActorService actorService, ActorId actorId, IClusterClient clusterClient = null, TelemetryClient telemetry = null)
            : base(actorService, actorId)
        {
            _kernel = new StandardKernel(new InstanceActorModule())
                .WithCore(actorService.Context, StateManager, clusterClient, telemetry)
                .WithMediator()
                .WithInstances();
            _context = _kernel.Get<InstanceContext>();
            _repository = _kernel.Get<IInstanceRepository>();
            _telemetryClient = _kernel.Get<TelemetryClient>();
        }

        public Task StartAsync(StartInstanceRequest request) =>
            _context.StartAsync(
                new StartInstance(
                    this.GetActorId().GetGuidId(), request.ServiceTypeUri, request.IsServiceStateful,
                    request.HasPersistedState, request.MinReplicas, request.TargetReplicas,
                    (PartitionSchemeDescription)Enum.Parse(typeof(PartitionSchemeDescription), request.PartitionScheme.ToString()),
                    request.ExpirationQuanta),
                _cancellation);

        public async Task RemoveAsync()
        {
            try
            {
                await UnregisterReminderAsync(GetReminder("expiration-quanta"));
            }
            catch (ReminderNotFoundException) { }
            await _context.RemoveAsync(new RemoveInstance(), _cancellation);
        }

        public async Task<OccupyResponse> OccupyAsync(OccupyRequest request)
        {
            var result = await _context.OccupyAsync(new OccupyInstance(this.GetActorId().GetGuidId(), request.PartitionId, request.ServiceInstanceName), _cancellation);
            //calculate a seed value to the nearest 100ms to stagger the due time of the different actors.
            //this prevents from all actors occupied within milliseconds of each other from all vacating at exactly the same time
            //which will cause a deadlock on the pool actor.
            var expirationQuanta = await _repository.GetExpirationQuantaAsync(_cancellation);
            var intervalMs = (int)Math.Round(expirationQuanta.TotalMilliseconds / 5);
            var dueMs = ((int)Math.Round((GetInstanceId().GetHashCode() % 1000) / 100.0) * 100) + intervalMs;
            await RegisterReminderAsync("expiration-quanta", null, TimeSpan.FromMilliseconds(dueMs), TimeSpan.FromMilliseconds(intervalMs));
            return new OccupyResponse(result.ServiceName);
        }

        public async Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request) =>
            (await _context.ReportActivityAsync(new ReportActivity(request.LastActiveUtc), _cancellation)).NextReportTime;

        public async Task VacateAsync()
        {
            try
            {
                await UnregisterReminderAsync(GetReminder("expiration-quanta"));
            }
            catch (ReminderNotFoundException) { }
            await _context.VacateAsync(new VacateInstance(), _cancellation);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            using (var request = _telemetryClient.StartOperation<RequestTelemetry>(reminderName))
            {
                try
                {
                    if (reminderName.Equals("expiration-quanta"))
                        await _context.CheckForExpirationAsync(new CheckForExpiration(this.GetActorId().GetGuidId()), _cancellation);
                }
                catch (Exception ex)
                {
                    request.Telemetry.Success = false;
                    _telemetryClient.TrackException(ex);
                }
            }
        }

        protected override Task OnActivateAsync() => _context.ActivateAsync(_cancellation);

        protected override Task OnDeactivateAsync() => _context.DeactivateAsync(_cancellation);

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