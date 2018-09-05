using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MongoDB.Bson;
using PoolManager.SDK;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolManager.Terminal
{
    class Program
    {
        public const string NoOpServiceTypeUri = "fabric:/PoolManager.Tests/NoOp";
        public static Uri NoOpApplicationName = new Uri("fabric:/PoolManager.Tests");
        public static string NoOpApplicationTypeName = "PoolManager.Tests";
        public const string NoOpApplicationVersion = "1.0.0";

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            WriteConsole("Press any key to start the test...");            

            IPoolProxy pools = new PoolProxy(new ActorProxyFactory());
            IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory();

            await Reset();
            await ResetPool(pools);
            Console.ReadKey();

            int users = 500;
            int serviceInstances = 385;
            TimeSpan testLength = new TimeSpan(0, 15, 0);
            TimeSpan rampUpTime = new TimeSpan(0, 12, 0);
            int rampUpInterval = 15000;
            int rampUpIntervals = (int)rampUpTime.TotalMilliseconds / rampUpInterval;
            double usersPerRampUpInterval = (double)users / (double)rampUpIntervals;

            LinkedList<Tuple<int, int>> usersByInterval = new LinkedList<Tuple<int, int>>();
            for (int i = 0; i < rampUpIntervals; i++)
            {
                var interval = i * rampUpInterval;
                var intervalUsers = (int)Math.Ceiling((i + 1) * usersPerRampUpInterval);
                usersByInterval.AddLast(new Tuple<int, int>(interval, intervalUsers));
            }

            var intervals = usersByInterval.GetEnumerator();
            intervals.MoveNext();

            string[] serviceInstanceNames = new string[serviceInstances];
            for (int i = 0; i < serviceInstances; i++)
            {
                serviceInstanceNames[i] = Guid.NewGuid().ToString();
            }

            WriteConsole($"Running activation load test for {serviceInstances} instances with {users} concurrentUsers");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            ConcurrentDictionary<ObjectId, Task<int>> activationTasks = new ConcurrentDictionary<ObjectId, Task<int>>();
            Random random = new Random();

            int nextInstanceCap = intervals.Current.Item2;
            while (timer.Elapsed < testLength)
            {
                bool eof = false;
                if (timer.ElapsedMilliseconds > intervals.Current.Item1 && !eof)
                {
                    eof = !intervals.MoveNext();
                    nextInstanceCap = Math.Min(intervals.Current.Item2, serviceInstanceNames.Length);
                }

                //this governs the degree of parallelism. can comment this out to make all run in parallel 
                var runningTasks = activationTasks.Where(t => !t.Value.IsCompleted).Select(t => t.Value);
                if (runningTasks.Count() >= intervals.Current.Item2)
                    await Task.WhenAny(runningTasks);

                var activationId = ObjectId.GenerateNewId();

                var activationTask = ActivateService(pools, serviceProxyFactory, NoOpServiceTypeUri, serviceInstanceNames[random.Next(nextInstanceCap)], activationId, intervals.Current.Item2);
                activationTasks.TryAdd(activationId, activationTask);
            }

            var activationResults = await Task.WhenAll(activationTasks.Select(t => t.Value));
            var native_time = activationResults.ToArray();
            var total_time = native_time.Sum();
            WriteConsole($"Average time: {(total_time / activationTasks.Count).ToString()}");
            WriteConsole($"90% of requests took less than (ms): {native_time[(int)(activationTasks.Count * 0.9)].ToString()}");
            WriteConsole($"95% of requests took less than (ms): {native_time[(int)(activationTasks.Count * 0.95)].ToString()}");
            WriteConsole($"99% of requests took less than (ms): {native_time[(int)(activationTasks.Count * 0.99)].ToString()}");
            //WriteConsole($"Raw results: {string.Join(", ", native_time.OrderBy(x => x))}");
            timer.Stop();            
            WriteConsole($"Activations finished in {timer.ElapsedMilliseconds} ms. Press x to exit");
            var key = Console.ReadKey();
            if (key.KeyChar != 'x')
                await MainAsync(args);
        }

        static async Task ResetPool(IPoolProxy pools)
        {
            WriteConsole("Removing old pool");
            await pools.StopPoolAsync(NoOpServiceTypeUri);
            await pools.DeletePoolAsync(NoOpServiceTypeUri);

            WriteConsole("Registering Service with Pool Manager...");
            var request = new StartPoolRequest(
                true,
                true,
                1,
                3,
                SDK.PartitionSchemeDescription.Singleton,
                500,
                60,
                5,
                TimeSpan.FromMinutes(5)
                );
            await pools.StartPoolAsync(NoOpServiceTypeUri, request);

            WriteConsole("Registration Completed");
        }

        static async Task Reset()
        {
            WriteConsole("Resetting test...");
            FabricClient client = new FabricClient();
            var types = await client.QueryManager.GetApplicationTypeListAsync(NoOpApplicationTypeName);
            if (types?.Any() ?? false)
            {
                var applications = await client.QueryManager.GetApplicationListAsync(NoOpApplicationName);
                if (applications?.Any() ?? false)
                {
                    DeleteApplicationDescription delete = new DeleteApplicationDescription(NoOpApplicationName);
                    await client.ApplicationManager.DeleteApplicationAsync(delete);
                }

                ApplicationDescription applicationDescription = new ApplicationDescription(NoOpApplicationName, NoOpApplicationTypeName, NoOpApplicationVersion);
                await client.ApplicationManager.CreateApplicationAsync(applicationDescription);
            }
            else
                throw new ArgumentException($"Application type '{NoOpApplicationTypeName}' not deployed to the cluster.");
        }

        static async Task<int> ActivateService(IPoolProxy pools, IServiceProxyFactory serviceProxyFactory, string serviceTypeName, string serviceInstanceName, ObjectId activationId, int users)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                var serviceInstance = await PooledServiceProxy<IServiceInstance>.Create(pools, serviceProxyFactory, serviceTypeName, serviceInstanceName);
                //verify the service instance uri returned exists
                //await serviceInstance.PingAsync();
            }
            catch (AggregateException ex)
            {
                WriteConsoleActivationStatus($"Failed with message {ex.InnerException.Message}", activationId, serviceInstanceName, users);
            }
            catch (Exception ex)
            {
                WriteConsoleActivationStatus($"Failed with message {ex.Message}", activationId, serviceInstanceName, users);
            }
            finally
            {
                timer.Stop();
                WriteConsoleActivationStatus($"Finished in {timer.ElapsedMilliseconds} ms", activationId, serviceInstanceName, users);
            }

            // 'think' time
            await Task.Delay(3000);
            return (int)timer.ElapsedMilliseconds;
        }

        static void WriteConsole(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, {message}");
        }

        static void WriteConsoleActivationStatus(string message, ObjectId activationId, string serviceInstanceName, int users)
        {
            WriteConsole($"{users}, {serviceInstanceName}, {activationId}, {message}");
        }
    }
}
