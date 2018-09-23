using CommandLine;
using MongoDB.Bson;
using PoolManager.Core.Mediators.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    [Verb("swarm")]
    public class Swarm : ICommand
    {
        public Swarm(
            string applicationType, 
            string applicationName, 
            string applicationVersion,
            string serviceTypeUri,
            int tenants,
            int users,
            int instances,
            double durationMinutes
            )
        {
            ApplicationType = applicationType;
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            ServiceTypeUri = serviceTypeUri;
            Tenants = tenants;
            Users = users;
            Instances = instances;
            DurationMinutes = durationMinutes;
        }
        [Option('a', "apptype", Default = Constants.NoOpApplicationTypeName)]
        public string ApplicationType { get; }
        [Option('n', "appname", Default = Constants.NoOpApplicationName)]
        public string ApplicationName { get; }
        [Option('v', "appversion", Default = Constants.NoOpApplicationVersion)]
        public string ApplicationVersion { get; }
        [Option('s', "service", Default = Constants.NoOpServiceTypeUri)]
        public string ServiceTypeUri { get; }
        [Option('t', "tenants", Default = 5)]
        public int Tenants { get; }
        [Option('u', "users", Default = 10)]
        public int Users { get; }
        [Option('i', "instances", Default = 8)]
        public int Instances { get; }
        [Option('d', "duration", Default = 1)]
        public double DurationMinutes { get; }

        public TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);
        public TimeSpan RampUpTime => TimeSpan.FromMilliseconds(Duration.TotalMilliseconds * .65);
        public TimeSpan RampUpInterval => TimeSpan.FromMilliseconds(Math.Min(Math.Max((RampUpTime.TotalMilliseconds * .05), 5000), 20000));
        public int RampUpIntervals => (int)Math.Ceiling(RampUpTime.TotalMilliseconds / RampUpInterval.TotalMilliseconds);
        public double RampUpUsers => Users / (double)RampUpIntervals;
    }

    public class SwarmHandler : IHandleCommand<Swarm>
    {
        private readonly ITerminal terminal;
        private readonly IHandleCommand<GetInstance> getInstanceHandler;
        private readonly IHandleCommand<RestartApplication> restartAppHandler;
        private readonly IHandleCommand<StartPool> startPoolHandler;
        private readonly IHandleCommand<EnsureAppReady> ensureAppReadyHandler;
        private readonly Random random = new Random();

        public SwarmHandler(ITerminal terminal, 
            IHandleCommand<GetInstance> getInstanceHandler, 
            IHandleCommand<RestartApplication> restartAppHandler, 
            IHandleCommand<StartPool> startPoolHandler,
            IHandleCommand<EnsureAppReady> ensureAppReadyHandler)
        {
            this.terminal = terminal;
            this.getInstanceHandler = getInstanceHandler;
            this.restartAppHandler = restartAppHandler;
            this.startPoolHandler = startPoolHandler;
            this.ensureAppReadyHandler = ensureAppReadyHandler;
        }

        public async Task ExecuteAsync(Swarm command, CancellationToken cancellationToken)
        {
            terminal.Write($"SWARM USERS: {command.Users}");
            terminal.Write($"SWARM TENANTS: {command.Tenants}");
            terminal.Write($"SWARM DURATION: {command.Duration}");
            terminal.Write($"SWARM RAMP UP TIME: {command.RampUpTime}");
            terminal.Write($"SWARM RAMP UP INTERVAL: {command.RampUpInterval}");
            terminal.Write($"SWARM RAMP UP INTERVALS: {command.RampUpIntervals}");
            terminal.Write($"SWARM RAMP UP USERS: {command.RampUpUsers}");

            //await Task.WhenAll(
            //    restartAppHandler.ExecuteAsync(new RestartApplication("PoolManager", "fabric:/PoolManager", command.ApplicationVersion), cancellationToken),
            //    restartAppHandler.ExecuteAsync(new RestartApplication(command.ApplicationType, command.ApplicationName, command.ApplicationVersion), cancellationToken)
            //);
            //await Task.Delay(15000);
            //await ensureAppReadyHandler.ExecuteAsync(new EnsureAppReady("fabric:/PoolManager"), cancellationToken);

            var intervals = GetIntervals(command.RampUpIntervals, command.RampUpInterval, command.RampUpUsers);
            var instances = GetInstances(command.Tenants, command.Instances, command.ServiceTypeUri);

            //await startPoolHandler.ExecuteAsync(new StartPool(command.ServiceTypeUri), cancellationToken);

            //await Task.Delay(15000);
            //await ensureAppReadyHandler.ExecuteAsync(new EnsureAppReady(command.ApplicationName), cancellationToken);

            intervals.MoveNext();
            ConcurrentDictionary<ObjectId, Task<SwarmExecution>> executionTasks = new ConcurrentDictionary<ObjectId, Task<SwarmExecution>>();
            int nextInstanceCap = intervals.Current.Users;

            Stopwatch timer = new Stopwatch();
            timer.Start();            
            while (timer.Elapsed < command.Duration)
            {
                bool eof = false;
                if (timer.Elapsed > intervals.Current.Interval && !eof)
                {
                    eof = !intervals.MoveNext();
                    nextInstanceCap = Math.Min(intervals.Current.Users, command.Instances);
                }

                //this governs the degree of parallelism. can comment this out to make all run in parallel 
                var runningTasks = executionTasks.Where(t => !t.Value.IsCompleted).Select(t => t.Value);
                if (runningTasks.Count() >= intervals.Current.Users)
                    try { await Task.WhenAny(runningTasks); } catch (ArgumentException) { }

                var executionTask = GetInstanceAsync(
                    intervals.Current.Users,
                    instances[random.Next(nextInstanceCap)],
                    cancellationToken);
                executionTasks.TryAdd(executionTask.Key, executionTask.Value);
            }

            await Task.WhenAll(executionTasks.Select(t => t.Value));
        }
        private KeyValuePair<ObjectId, Task<SwarmExecution>> GetInstanceAsync(int users, SwarmInstance instance, CancellationToken cancellationToken)
        {
            var executionId = ObjectId.GenerateNewId();
            var executionTask = GetInstanceAsync(users, executionId, instance, cancellationToken);
            return new KeyValuePair<ObjectId, Task<SwarmExecution>>(executionId, executionTask);
        }
        private async Task<SwarmExecution> GetInstanceAsync(int users, ObjectId executionId, SwarmInstance instance, CancellationToken cancellationToken)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Exception exception = null;
            try
            {
                await getInstanceHandler.ExecuteAsync(new GetInstance(instance.PartitionId, instance.InstanceName), cancellationToken);
            }
            catch(Exception ex)
            {
                exception = ex;
            }
            finally
            {
                timer.Stop();
            }
            terminal.Write($"{users}, {executionId}, {timer.Elapsed}, {exception?.Message ?? "success"}");            
            return new SwarmExecution(timer.Elapsed, exception);
        }
        private LinkedList<SwarmInterval>.Enumerator GetIntervals(int rampUpIntervals, TimeSpan rampUpInterval, double rampUpUsers)
        {
            LinkedList<SwarmInterval> usersByInterval = new LinkedList<SwarmInterval>();
            for (int i = 0; i < rampUpIntervals; i++)
            {
                var interval = TimeSpan.FromMilliseconds(i * rampUpInterval.TotalMilliseconds);
                var intervalUsers = (int)Math.Round((i + 1) * rampUpUsers, 0);
                usersByInterval.AddLast(new SwarmInterval(interval, intervalUsers));
            }
            return usersByInterval.GetEnumerator();
        }
        private SwarmInstance[] GetInstances(int numberOfPartitions, int numberOfInstances, string serviceTypeUri)
        {
            string[] partitions = new string[numberOfPartitions];
            for (int i = 0; i < numberOfPartitions; i++) partitions[i] = Guid.NewGuid().ToString();

            SwarmInstance[] instances = new SwarmInstance[numberOfInstances];
            for (int i = 0; i < numberOfInstances; i++)
            {
                instances[i] = new SwarmInstance(serviceTypeUri, partitions[i % numberOfPartitions], Guid.NewGuid().ToString());
            }
            return instances;
        }
    }

    public class SwarmExecution
    {
        public SwarmExecution(TimeSpan elapsed, Exception exception = null)
        {
            Elapsed = elapsed;
            Exception = exception;
        }

        public TimeSpan Elapsed { get; }
        public bool Failed => Exception != null;
        public Exception Exception { get; }
    }

    public class SwarmInterval
    {
        public SwarmInterval(TimeSpan interval, int users)
        {
            Interval = interval;
            Users = users;
        }

        public TimeSpan Interval { get; }
        public int Users { get; }
    }

    public class SwarmInstance
    {
        public SwarmInstance(string serviceTypeUri, string partitionId, string instanceName)
        {
            PartitionId = partitionId;
            ServiceTypeUri = serviceTypeUri;
            InstanceName = instanceName;
        }

        public string PartitionId { get; }
        public string ServiceTypeUri { get; }
        public string InstanceName { get; }
    }
}
