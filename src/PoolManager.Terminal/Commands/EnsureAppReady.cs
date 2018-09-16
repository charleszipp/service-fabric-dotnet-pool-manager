using CommandLine;
using PoolManager.Core.Commands;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    [Verb("ensure-app-ready")]
    public class EnsureAppReady : ICommand
    {
        public EnsureAppReady(string applicationName)
        {
            ApplicationName = applicationName;
        }

        [Option('n', "name", Default = Constants.NoOpApplicationName)]
        public string ApplicationName { get; }

        public Uri ApplicationUri => new Uri(ApplicationName);
    }

    public class EnsureAppReadyHandler : IHandleCommand<EnsureAppReady>
    {
        private readonly FabricClient _fabricClient;
        private readonly ITerminal _terminal;

        public EnsureAppReadyHandler(FabricClient fabricClient, ITerminal terminal)
        {
            _fabricClient = fabricClient ?? new FabricClient();
            _terminal = terminal;
        }
        public async Task ExecuteAsync(EnsureAppReady command, CancellationToken cancellationToken)
        {
            _terminal.Write($"Ensuring all services healthy for app {command.ApplicationName}");
            var unhealthyCount = (await _fabricClient.QueryManager.GetServiceListAsync(command.ApplicationUri)).Count(x => x.HealthState != HealthState.Ok);
            var task = WaitForAllServicesToBecomeHealthy();
            if (await Task.WhenAny(task, Task.Delay(60000)) != task)
                throw new Exception($"Gave up waiting for {command.ApplicationUri} to become healthy");
            async Task WaitForAllServicesToBecomeHealthy()
            {                
                while (unhealthyCount > 0)
                {
                    unhealthyCount = (await _fabricClient.QueryManager.GetServiceListAsync(command.ApplicationUri))
                        .Count(x => x.HealthState != HealthState.Ok);
                    _terminal.Write($"app {command.ApplicationName} still has {unhealthyCount} services unhealthy...");
                    await Task.Delay(1000);
                }
            }
        }
    }
}
