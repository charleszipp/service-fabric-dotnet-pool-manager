﻿using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolStateActive : PoolState
    {
        public override PoolStates State => PoolStates.Active;

        public override async Task<Guid> GetAsync(PoolContext context, GetInstanceRequest request)
        {
            var poolInstances = await context.GetPoolInstancesAsync();
            var configuration = await context.GetPoolConfigurationAsync();

            //if we have one occupied...
            if (poolInstances.OccupiedInstances.ContainsKey(request.ServiceInstanceName))
            {
                //return the instance id
                return poolInstances.OccupiedInstances[request.ServiceInstanceName];
            }
            else
            {
                //if one has not been occupied but, we have a vacant one available
                Guid instanceId = Guid.Empty;
                if (poolInstances.VacantInstances.TryDequeue(out instanceId))
                {
                    await context.InstanceProxy.OccupyAsync(instanceId, new SDK.Instances.Requests.OccupyRequest(request.ServiceInstanceName, configuration.ExpirationQuanta));
                    poolInstances.OccupiedInstances[request.ServiceInstanceName] = instanceId;
                    return instanceId;
                }
                else
                {
                    //else we dont have a vacant one available so start it from scratch
                    //this would likely happen if we are at or above the max pool size
                    //todo: instrument this situation so we can track how often we are hitting the cap
                    await context.AddInstanceAsAsync(request.ServiceInstanceName, configuration, poolInstances);
                    return poolInstances.OccupiedInstances[request.ServiceInstanceName];
                }
            }
        }

        public override async Task VacateInstanceAsync(PoolContext context, VacateInstanceRequest request)
        {
            var poolInstances = await context.GetPoolInstancesAsync();
            var poolConfig = await context.GetPoolConfigurationAsync();
            if (poolInstances.OccupiedInstances.TryRemove(request.ServiceInstanceName, out var instanceId))
            {
                if (poolInstances.VacantInstances.Count >= poolConfig.IdleServicesPoolSize)
                {
                    //todo: instrument this to track how many services are being removed during vacate
                    await context.InstanceProxy.RemoveAsync(instanceId);
                    poolInstances.RemovedInstances.Enqueue(instanceId);
                }                    
                else
                {
                    //todo: instrument this to track how many instances we are vacating and sending back to the pool
                    await context.InstanceProxy.VacateAsync(instanceId);
                    poolInstances.VacantInstances.Enqueue(instanceId);
                }

                await context.SetPoolInstancesAsync(poolInstances);
            }
            else
            {
                context.TelemetryClient.TrackTrace("Attempt was made to vacate a service that is not currently occupied");
            }
        }
    }
}