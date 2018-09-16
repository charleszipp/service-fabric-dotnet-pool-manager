using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PoolManager.Core.Mediators.Commands;
using PoolManager.SDK;
using PoolManager.SDK.Pools;

namespace PoolManager.Terminal.Commands
{
    [Verb("get-instance")]
    public class GetInstance : ICommand
    {
        public GetInstance(
            string poolId,
            string name)
        {
            PoolId = poolId;
            Name = name;
        }

        [Option('p', "pool", Default = Constants.NoOpServiceTypeUri)]
        public string PoolId { get; }
        [Option('n', "name")]
        public string Name { get; }
    }

    public class GetInstanceHandler : IHandleCommand<GetInstance>
    {
        private readonly IPoolProxy _pools;
        private readonly ITerminal _terminal;

        public GetInstanceHandler(IPoolProxy pools, ITerminal terminal)
        {
            _pools = pools;
            _terminal = terminal;
        }

        public async Task ExecuteAsync(GetInstance command, CancellationToken cancellationToken)
        {
            var response = await _pools.GetInstanceAsync(command.PoolId, new SDK.Pools.Requests.GetInstanceRequest(command.Name));
            var proxy = ServiceProxy.Create<IServiceInstance>(response.ServiceInstanceUri);
            await proxy.PingAsync();
        }
    }
}
