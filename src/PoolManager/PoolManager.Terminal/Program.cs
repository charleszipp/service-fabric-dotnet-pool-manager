using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
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
                var instanceId = Guid.NewGuid();
                var instance = ActorProxy.Create<IInstance>(new ActorId(instanceId), "PoolManager", "InstanceActorService");
                await instance.StartAsync(new StartRequest(
                    "fabric:/ServicePoolManagerLoadTestHarness/NoOpType", true, true, 3, 3, SDK.PartitionSchemeDescription.Singleton
                    ));
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
