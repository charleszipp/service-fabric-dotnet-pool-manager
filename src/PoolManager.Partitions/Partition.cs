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

namespace PoolManager.Partitions
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Partition : Actor, IPartition, IRemindable
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IInstanceProxy instances;
        private const string CleanupRemovedInstancesReminderKey = "cleanup-removed-instances";

        public Partition(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient, IInstanceProxy instances)
            : base(actorService, actorId)
        {
            this.telemetryClient = telemetryClient;
            this.instances = instances;
        }

        public async Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request)
        {
            var instance = await TryGetAsync(request.ServiceTypeUri, request.InstanceName);
            if (instance.HasValue)
                return new GetInstanceResponse(instance.Value.ServiceName);
            else
            {
                //todo: ask pool for a vacant instance
                //todo: occupy vacant instance from pool
                //todo: return service name of occupied instance
                //todo: if occupy fails or takes over a certain time, mark the instance for deletion and retry
                //todo: add the occupied instance back to the state manager

                throw new ArgumentException("Unable to find a mapped instance for given pool and name");
            }
        }

        public async Task VacateInstanceAsync(VacateInstanceRequest request)
        {
            var instance = await TryGetAsync(request.ServiceTypeUri, request.InstanceName);
            if (instance.HasValue)
            {
                await instances.VacateAsync(instance.Value.Id);
                await EnqueueForDeleteAsync(instance.Value.Id);
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
                await RegisterReminderAsync(CleanupRemovedInstancesReminderKey, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            }
        }

        private async Task EnqueueForDeleteAsync(Guid instanceId) => 
            (await DeleteQueue).Enqueue(instanceId);

        private Task<Queue<Guid>> DeleteQueue => 
            StateManager.GetOrAddStateAsync("deletequeue", new Queue<Guid>());

        private Task<ConditionalValue<MappedInstance>> TryGetAsync(string serviceTypeUri, string instanceName) => 
            StateManager.TryGetStateAsync<MappedInstance>(GetStateName(serviceTypeUri, instanceName));

        private string GetStateName(string serviceTypeUri, string serviceInstanceName) => 
            $"{serviceTypeUri.TrimEnd('/')}/{serviceInstanceName}";

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
    }
}