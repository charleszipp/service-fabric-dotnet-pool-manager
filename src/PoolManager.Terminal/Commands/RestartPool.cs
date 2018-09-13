using CommandLine;
using PoolManager.SDK;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    [Verb("restart-pool")]
    public class RestartPool : ICommand
    {
        public RestartPool(
            string poolId,
            string serviceTypeUri,
            bool isServiceStateful,
            bool hasPersistedState,
            int minReplicas,
            int targetReplicas,
            int maxPoolSize,
            int idleServicesPoolSize,
            int servicesAllocationBlockSize,
            PartitionSchemeDescription partitionScheme,
            TimeSpan? expirationQuanta
            )
        {
            PoolId = poolId;
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

        [Option('p', "pool", Default = Constants.NoOpServiceTypeUri)]
        public string PoolId { get; }

        [Option('u', "uri", Default = Constants.NoOpServiceTypeUri)]
        public string ServiceTypeUri { get; }

        [Option('s', "stateful", Default = true)]
        public bool IsServiceStateful { get; }

        [Option('h', "persisted", Default = true)]
        public bool HasPersistedState { get; }

        [Option('r', "min", Default = 1)]
        public int MinReplicas { get; }

        [Option('t', "target", Default = 3)]
        public int TargetReplicas { get; }

        [Option('m', "max", Default = int.MaxValue)]
        public int MaxPoolSize { get; }

        [Option('i', "idle", Default = 10)]
        public int IdleServicesPoolSize { get; }

        [Option('b', "blocks", Default = 5)]
        public int ServicesAllocationBlockSize { get; }

        [Option('n', "partition", Default = PartitionSchemeDescription.Singleton)]
        public PartitionSchemeDescription PartitionScheme { get; }

        [Option('e', "expiry")]
        public TimeSpan? ExpirationQuanta { get; }
    }

    public class RestartPoolHandler : IHandleCommand<RestartPool>
    {
        private readonly IPoolProxy _pools;
        private readonly ITerminal _terminal;

        public RestartPoolHandler(IPoolProxy pools, ITerminal terminal)
        {
            _pools = pools;
            _terminal = terminal;
        }

        public async Task ExecuteAsync(RestartPool command, CancellationToken cancellationToken)
        {
            if (await _pools.IsActive(command.PoolId))
            {
                _terminal.Write("pool started, stopping and deleting pool...");
                await _pools.StopPoolAsync(command.PoolId);
                await _pools.DeletePoolAsync(command.PoolId);
                _terminal.Write("pool removed");
            }

            _terminal.Write("starting new pool...");
            var request = new StartPoolRequest(
                isServiceStateful: command.IsServiceStateful,
                hasPersistedState: command.HasPersistedState,
                minReplicas: command.MinReplicas,
                targetReplicas: command.TargetReplicas,
                partitionScheme: command.PartitionScheme,
                maxPoolSize: command.MaxPoolSize,
                idleServicesPoolSize: command.IdleServicesPoolSize,
                servicesAllocationBlockSize: command.ServicesAllocationBlockSize,
                expirationQuanta: command.ExpirationQuanta
                );
            await _pools.StartPoolAsync(command.PoolId, request);
            _terminal.Write("pool started");
        }
    }
}
