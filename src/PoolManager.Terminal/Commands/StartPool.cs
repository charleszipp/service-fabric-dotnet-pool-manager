using CommandLine;
using PoolManager.Core.Mediators.Commands;
using PoolManager.SDK;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    [Verb("start-pool")]
    public class StartPool : ICommand
    {
        private class Defaults
        {
            public const string ServiceTypeUri = Constants.NoOpServiceTypeUri;
            public const bool IsServiceStateful = true;
            public const bool HasPersistedState = true;
            public const int MinReplicas = 1;
            public const int TargetReplicas = 3;
            public const int MaxPoolSize = 20;
            public const int IdleServicesPoolSize = 5;
            public const int ServicesAllocationBlockSize = 2;
            public const PartitionSchemeDescription PartitionScheme = PartitionSchemeDescription.Singleton;
            public const double ExpirationQuanta = 15;
        }

        public StartPool(
            string serviceTypeUri = Defaults.ServiceTypeUri,
            bool isServiceStateful = Defaults.IsServiceStateful,
            bool hasPersistedState = Defaults.HasPersistedState,
            int minReplicas = Defaults.MinReplicas,
            int targetReplicas = Defaults.TargetReplicas,
            int maxPoolSize = Defaults.MaxPoolSize,
            int idleServicesPoolSize = Defaults.IdleServicesPoolSize,
            int servicesAllocationBlockSize = Defaults.ServicesAllocationBlockSize,
            PartitionSchemeDescription partitionScheme = Defaults.PartitionScheme,
            double expirationQuanta = Defaults.ExpirationQuanta
            )
        {
            ServiceTypeUri = serviceTypeUri;
            IsServiceStateful = isServiceStateful;
            HasPersistedState = hasPersistedState;
            MinReplicas = minReplicas;
            TargetReplicas = targetReplicas;
            MaxPoolSize = maxPoolSize;
            IdleServicesPoolSize = idleServicesPoolSize;
            PartitionScheme = partitionScheme;
            ServicesAllocationBlockSize = servicesAllocationBlockSize;
            ExpirationQuanta = expirationQuanta;
        }

        [Option('u', "uri", Default = Defaults.ServiceTypeUri)]
        public string ServiceTypeUri { get; }

        [Option('s', "stateful", Default = Defaults.IsServiceStateful)]
        public bool IsServiceStateful { get; }

        [Option('h', "persisted", Default = Defaults.HasPersistedState)]
        public bool HasPersistedState { get; }

        [Option('r', "min", Default = Defaults.MinReplicas)]
        public int MinReplicas { get; }

        [Option('t', "target", Default = Defaults.TargetReplicas)]
        public int TargetReplicas { get; }

        [Option('m', "max", Default = Defaults.MaxPoolSize)]
        public int MaxPoolSize { get; }

        [Option('i', "idle", Default = Defaults.IdleServicesPoolSize)]
        public int IdleServicesPoolSize { get; }

        [Option('b', "blocks", Default = Defaults.ServicesAllocationBlockSize)]
        public int ServicesAllocationBlockSize { get; }

        [Option('n', "partition", Default = Defaults.PartitionScheme)]
        public PartitionSchemeDescription PartitionScheme { get; }

        [Option('e', "expiry", Default = Defaults.ExpirationQuanta)]
        public double ExpirationQuanta { get; }
    }

    public class StartPoolHandler : IHandleCommand<StartPool>
    {
        private readonly IPoolProxy _pools;
        private readonly ITerminal _terminal;

        public StartPoolHandler(IPoolProxy pools, ITerminal terminal)
        {
            _pools = pools;
            _terminal = terminal;
        }

        public async Task ExecuteAsync(StartPool command, CancellationToken cancellationToken)
        {
            _terminal.Write($"{command.ServiceTypeUri}, starting pool");
            var request = new StartPoolRequest(
                serviceTypeUri: command.ServiceTypeUri,
                isServiceStateful: command.IsServiceStateful,
                hasPersistedState: command.HasPersistedState,
                minReplicas: command.MinReplicas,
                targetReplicas: command.TargetReplicas,
                partitionScheme: command.PartitionScheme,
                maxPoolSize: command.MaxPoolSize,
                idleServicesPoolSize: command.IdleServicesPoolSize,
                servicesAllocationBlockSize: command.ServicesAllocationBlockSize,
                expirationQuanta: TimeSpan.FromMinutes(command.ExpirationQuanta)
                );
            await _pools.StartPoolAsync(command.ServiceTypeUri, request);
            _terminal.Write($"{command.ServiceTypeUri}, pool started. pausing for service creation");
            _terminal.Write($"{command.ServiceTypeUri}, pool ready.");
        }
    }
}
