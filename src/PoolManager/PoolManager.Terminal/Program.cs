﻿using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolManager.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start");
            Console.ReadKey();

            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                var pool = ActorProxy.Create<IPool>(new ActorId("fabric:/PoolManager.Tests/NoOp"), "PoolManager", "PoolActorService");
                await pool.StartAsync(new SDK.Pools.Requests.StartPoolRequest(
                    true,
                    true,
                    1,
                    3,
                    SDK.PartitionSchemeDescription.Singleton,
                    10,
                    1,
                    1,
                    TimeSpan.FromMinutes(1)
                    ));

                await pool.GetAsync(new SDK.Pools.Requests.GetInstanceRequest(Guid.NewGuid().ToString()));
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
