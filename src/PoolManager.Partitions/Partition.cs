using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Partitions.Requests;
using PoolManager.SDK.Partitions.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoolManager.Partitions.Models;
using PoolManager.SDK.Pools;
using Ninject;
using PoolManager.Domains.Partitions;
using PoolManager.Core;
using PoolManager.Core.Mediators;
using System.Threading;
using PoolManager.Domains.Partitions.Interfaces;

namespace PoolManager.Partitions
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Partition : Actor, IPartition, IRemindable
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IInstanceProxy instances;
        private readonly IPoolProxy pools;
        private readonly Mediator mediator;
        private readonly IKernel _kernel;
        private const string CleanupRemovedInstancesReminderKey = "cleanup-removed-instances";

        public Partition(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {            
            _kernel = new StandardKernel(new PartitionActorModule())
                .WithCore(actorService.Context, StateManager)
                .WithMediator()
                .WithPartitions<PartitionRepository, PopVacantInstanceProxy, OccupyInstanceProxy>();

            mediator = _kernel.Get<Mediator>();
            telemetryClient = _kernel.Get<TelemetryClient>();
            instances = _kernel.Get<IInstanceProxy>();
            pools = _kernel.Get<IPoolProxy>();
        }

        public async Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request)
        {
            var result = await mediator.ExecuteAsync(new GetInstance(request.ServiceTypeUri, request.InstanceName, this.GetActorId().GetStringId()), default(CancellationToken));
            return new GetInstanceResponse(result.ServiceName);
        }

        public async Task VacateInstanceAsync(VacateInstanceRequest request)
        {
            var exists = await TryRemoveAsync(request.ServiceTypeUri, request.InstanceName);
            if (exists)
            {
                await instances.VacateAsync(request.InstanceId);
                await EnqueueForDeleteAsync(request.InstanceId);
            }
            else
                throw new ArgumentException($"Unable to vacate. No mapped instance found for provided {nameof(request.ServiceTypeUri)} and {nameof(request.InstanceName)}.");
        }

        public async Task CleanupDeletedInstancesAsync()
        {
            var partitionId = this.GetActorId().ToString();
            var deleteQueue = await DeleteQueue;
            telemetryClient.GetMetric("pools.removed.size", nameof(partitionId)).TrackValue(deleteQueue.Count, partitionId);

            var deletes = new List<Task>();
            try
            {
                while (deleteQueue.Count > 0)
                {
                    deletes.Add(instances.DeleteAsync(deleteQueue.Dequeue()));
                }
                await Task.WhenAll(deletes);
            }
            finally
            {
                telemetryClient.GetMetric("pools.removed.completed", nameof(partitionId)).TrackValue(deletes.Count(d => d.IsCompleted), partitionId);
                telemetryClient.GetMetric("pools.removed.failed", nameof(partitionId)).TrackValue(deletes.Count(d => d.IsFaulted), partitionId);
            }
        }

        protected override async Task OnActivateAsync()
        {
            try
            {
                GetReminder(CleanupRemovedInstancesReminderKey);
            }
            catch(ReminderNotFoundException)
            {
                await RegisterReminderAsync(CleanupRemovedInstancesReminderKey, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            }
        }

        private async Task EnqueueForDeleteAsync(Guid instanceId) => 
            (await DeleteQueue).Enqueue(instanceId);

        private Task<Queue<Guid>> DeleteQueue => 
            StateManager.GetOrAddStateAsync("deletequeue", new Queue<Guid>());

        private Task<ConditionalValue<MappedInstance>> TryGetAsync(string serviceTypeUri, string instanceName) => 
            StateManager.TryGetStateAsync<MappedInstance>(GetStateName(serviceTypeUri, instanceName));

        private Task SetMappedInstanceAsync(string serviceTypeUri, string instanceName, Guid instanceId, Uri serviceName) =>
            StateManager.SetStateAsync(GetStateName(serviceTypeUri, instanceName), new MappedInstance(instanceId, serviceName, instanceName));

        private Task<bool> TryRemoveAsync(string serviceTypeUri, string instanceName) =>
            StateManager.TryRemoveStateAsync(GetStateName(serviceTypeUri, instanceName));

        private string GetStateName(string serviceTypeUri, string serviceInstanceName) => 
            $"{serviceTypeUri}?{serviceInstanceName}";

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            using (var request = telemetryClient.StartOperation<RequestTelemetry>(reminderName))
            {
                request.Telemetry.Properties.Add("DueTime", dueTime.ToString());
                request.Telemetry.Properties.Add("Interval", period.ToString());
                try
                {
                    switch (reminderName)
                    {
                        case CleanupRemovedInstancesReminderKey:
                            await CleanupDeletedInstancesAsync();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    request.Telemetry.Success = false;
                    telemetryClient.TrackException(ex);
                }
            }
        }

        public async Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(GetOccupiedInstancesRequest request)
        {
            var stateNames = (await StateManager.GetStateNamesAsync())
                .Where(name => name.StartsWith(request.ServiceTypeUri))
                .ToList();
            var partitionId = this.GetActorId().GetStringId();
            var occupiedInstances = (await Task.WhenAll(stateNames.Select(name => StateManager.GetStateAsync<MappedInstance>(name))))
                .Select(i => new OccupiedInstance(i.Id, i.ServiceName, i.InstanceName, partitionId))
                .ToList();

            return new GetOccupiedInstancesResponse(occupiedInstances);
        }
    }
}