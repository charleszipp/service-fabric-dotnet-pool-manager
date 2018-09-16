using CommandLine;
using MongoDB.Bson;
using PoolManager.Core.Commands;
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
        public int RampUpUsers => Users / RampUpIntervals;
    }

    public class SwarmHandler : IHandleCommand<Swarm>
    {
        private readonly ITerminal terminal;
        private readonly IHandleCommand<GetInstance> getInstanceHandler;
        private readonly IHandleCommand<RestartApplication> restartAppHandler;
        private readonly IHandleCommand<RestartPool> restartPoolHandler;
        private readonly IHandleCommand<EnsureAppReady> ensureAppReadyHandler;
        private readonly Random random = new Random();

        public SwarmHandler(ITerminal terminal, 
            IHandleCommand<GetInstance> getInstanceHandler, 
            IHandleCommand<RestartApplication> restartAppHandler, 
            IHandleCommand<RestartPool> restartPoolHandler,
            IHandleCommand<EnsureAppReady> ensureAppReadyHandler)
        {
            this.terminal = terminal;
            this.getInstanceHandler = getInstanceHandler;
            this.restartAppHandler = restartAppHandler;
            this.restartPoolHandler = restartPoolHandler;
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

            await Task.WhenAll(
                restartAppHandler.ExecuteAsync(new RestartApplication("PoolManager", "fabric:/PoolManager", command.ApplicationVersion), cancellationToken),
                restartAppHandler.ExecuteAsync(new RestartApplication(command.ApplicationType, command.ApplicationName, command.ApplicationVersion), cancellationToken)
            );
            await Task.Delay(15000);
            await ensureAppReadyHandler.ExecuteAsync(new EnsureAppReady("fabric:/PoolManager"), cancellationToken);

            var intervals = GetIntervals(command.RampUpIntervals, command.RampUpInterval, command.RampUpUsers);
            var instanceNames = GetInstanceNames(command.Instances);
            var tenants = GetTenantIds(command.Tenants);
            var users = GetUsers(command.Users, command.ServiceTypeUri, tenants);

            foreach (var poolId in users.Select(u => u.PoolId).Distinct())
            {
                await restartPoolHandler.ExecuteAsync(
                    new RestartPool(poolId, command.ServiceTypeUri),
                    cancellationToken
                );
            }
            await Task.Delay(15000);
            await ensureAppReadyHandler.ExecuteAsync(new EnsureAppReady(command.ApplicationName), cancellationToken);

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
                    nextInstanceCap = Math.Min(intervals.Current.Users, instanceNames.Length);
                }

                //this governs the degree of parallelism. can comment this out to make all run in parallel 
                var runningTasks = executionTasks.Where(t => !t.Value.IsCompleted).Select(t => t.Value);
                if (runningTasks.Count() >= intervals.Current.Users)
                    try { await Task.WhenAny(runningTasks); } catch (ArgumentException) { }

                var executionTask = GetInstanceAsync(
                    intervals.Current.Users,
                    users[random.Next(intervals.Current.Users)].PoolId, 
                    instanceNames[random.Next(nextInstanceCap)],
                    cancellationToken);
                executionTasks.TryAdd(executionTask.Key, executionTask.Value);
            }

            await Task.WhenAll(executionTasks.Select(t => t.Value));
        }
        private KeyValuePair<ObjectId, Task<SwarmExecution>> GetInstanceAsync(int users, string poolId, string instanceName, CancellationToken cancellationToken)
        {
            var executionId = ObjectId.GenerateNewId();
            var executionTask = GetInstanceAsync(users, executionId, poolId, instanceName, cancellationToken);
            return new KeyValuePair<ObjectId, Task<SwarmExecution>>(executionId, executionTask);
        }
        private async Task<SwarmExecution> GetInstanceAsync(int users, ObjectId executionId, string poolId, string instanceName, CancellationToken cancellationToken)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Exception exception = null;
            try
            {
                await getInstanceHandler.ExecuteAsync(new GetInstance(poolId, instanceName), cancellationToken);
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
            await Task.Delay(3000);
            return new SwarmExecution(timer.Elapsed, exception);
        }
        private LinkedList<SwarmInterval>.Enumerator GetIntervals(int rampUpIntervals, TimeSpan rampUpInterval, int rampUpUsers)
        {
            LinkedList<SwarmInterval> usersByInterval = new LinkedList<SwarmInterval>();
            for (int i = 0; i < rampUpIntervals; i++)
            {
                var interval = TimeSpan.FromMilliseconds(i * rampUpInterval.TotalMilliseconds);
                var intervalUsers = (i + 1) * rampUpUsers;
                usersByInterval.AddLast(new SwarmInterval(interval, intervalUsers));
            }
            return usersByInterval.GetEnumerator();
        }
        private string[] GetInstanceNames(int numberOfInstances)
        {
            string[] instanceNames = new string[numberOfInstances];
            for (int i = 0; i < numberOfInstances; i++) instanceNames[i] = Guid.NewGuid().ToString();
            return instanceNames;
        }
        private string[] GetTenantIds(int numberOfTenants)
        {
            string[] tenantIds = new string[numberOfTenants];
            for (int i = 0; i < numberOfTenants; i++) tenantIds[i] = Guid.NewGuid().ToString();
            return tenantIds;
        }
        private SwarmUser[] GetUsers(int numberOfUsers, string serviceTypeUri, string[] tenants)
        {
            SwarmUser[] users = new SwarmUser[numberOfUsers];
            for (int i = 0; i < numberOfUsers; i++) users[i] = new SwarmUser(Guid.NewGuid().ToString(), serviceTypeUri, tenants[i % tenants.Length]);
            return users;
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

    public class SwarmUser
    {
        public SwarmUser(string userId, string serviceTypeUri, string tenantId)
        {
            UserId = userId;
            PoolId = $"{serviceTypeUri}?{tenantId}";
        }

        public string UserId { get; }
        public string PoolId { get; }
    }
}
