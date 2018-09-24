using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using PoolManager.Monitor.Extensions;
using PoolManager.Monitor.Interfaces;

namespace PoolManager.Monitor
{
    public class PoolManagerCommandWorker
    {
        private readonly TelemetryClient _telemetryClient;
        private const int NumberOfConcurrentCommands = 10;

        public PoolManagerCommandWorker(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void Run(CancellationToken cancellationToken, BlockingCollection<ICommand> commands)
        {
            _telemetryClient.TrackThreadState("PoolManagerCommandWorker::Run", "Running");
            IList<Task> tasks = new List<Task>();
            while (!cancellationToken.IsCancellationRequested)
            {
                if (commands.TryTake(out var command, -1, cancellationToken))
                {
                    tasks.Add(command.ExecuteAsync(cancellationToken));
                }

                // This is not perfect and will pause even if the commands are not really batched, but it
                // does the trick of helping to ensure that there are not too many tasks running at once.
                if (tasks.Count >= NumberOfConcurrentCommands)
                {
                    Task.WaitAll(tasks.ToArray(), cancellationToken);
                    tasks.Clear();
                }
            }

            _telemetryClient.TrackThreadState("PoolManagerCommandWorker::Run", "Stopped");
        }
    }
}