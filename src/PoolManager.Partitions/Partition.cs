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
using PoolManager.SDK.Instances.Requests;

namespace PoolManager.Partitions
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Partition : Actor, IPartition, IRemindable
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IInstanceProxy instances;
        private readonly IPoolProxy pools;
        private const string CleanupRemovedInstancesReminderKey = "cleanup-removed-instances";

        public Partition(ActorService actorService, ActorId actorId, TelemetryClient telemetryClient, IInstanceProxy instances, IPoolProxy pools)
            : base(actorService, actorId)
        {
            this.telemetryClient = telemetryClient;
            this.instances = instances;
            this.pools = pools;
        }

        public async Task<GetInstanceResponse> GetInstanceAsync(GetInstanceRequest request)
        {
            var instance = await TryGetAsync(request.ServiceTypeUri, request.InstanceName);
            if (instance.HasValue)
                return new GetInstanceResponse(instance.Value.ServiceName);
            else
            {
                var popVacantInstanceResponse = await pools.PopVacantInstanceAsync(request.ServiceTypeUri, new SDK.Pools.Requests.PopVacantInstanceRequest());
                if(popVacantInstanceResponse.InstanceId.HasValue)
                {
                    var occupyResponse = await instances.OccupyAsync(popVacantInstanceResponse.InstanceId.Value, new OccupyRequest(this.GetActorId().GetStringId(), request.InstanceName));
                    await SetMappedInstanceAsync(request.ServiceTypeUri, request.InstanceName, popVacantInstanceResponse.InstanceId.Value, occupyResponse.ServiceName);
                    return new GetInstanceResponse(occupyResponse.ServiceName);
                    //todo: if occupy fails or takes over a certain time, mark the instance for deletion and retry

                    throw new Exception("Pool was unable to provide a vacant instance to occupy.");
                }

                throw new ArgumentException("Unable to find a mapped instance for given pool and name");
            }
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
            StateManager.SetStateAsync(GetStateName(serviceTypeUri, instanceName), new MappedInstance(instanceId, serviceName));

        private Task<bool> TryRemoveAsync(string serviceTypeUri, string instanceName) =>
            StateManager.TryRemoveStateAsync(GetStateName(serviceTypeUri, instanceName));

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

        public async Task<GetOccupiedInstancesResponse> GetOccupiedInstancesAsync(GetOccupiedInstancesRequest request)
        {
            var stateNames = (await StateManager.GetStateNamesAsync())
                .Where(name => name.StartsWith(request.ServiceTypeUri))
                .ToList();

            var occupiedInstances = (await Task.WhenAll(stateNames.Select(name => StateManager.GetStateAsync<MappedInstance>(name))))
                .Select(mappedInstance => mappedInstance.Id)
                .ToList();

            return new GetOccupiedInstancesResponse(occupiedInstances);
        }
    }
}