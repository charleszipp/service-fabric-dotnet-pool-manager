using CommandLine;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Ninject;
using PoolManager.SDK.Pools;
using PoolManager.Terminal.Builders;
using PoolManager.Terminal.Commands;
using PoolManager.Terminal.Resolvers;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            var cancellation = new CancellationTokenSource();

            var kernel = new StandardKernel();
            kernel.Bind<FabricClient>().ToSelf().InSingletonScope();
            kernel.Bind<ITerminal>().To<Terminal>();
            kernel.Bind<IActorProxyFactory>().ToMethod(ctx => new ActorProxyFactory()).InSingletonScope();
            kernel.Bind<IPoolProxy>().To<PoolProxy>();
            var resolver = new NinjectDependencyResolver(kernel);
            
            var pools = new PoolsBuilder(resolver)
                .WithCommandHandler<RestartApplicationHandler, RestartApplication>()
                .WithCommandHandler<RestartPoolHandler, RestartPool>()
                .WithCommandHandler<GetInstanceHandler, GetInstance>()
                .WithCommandHandler<SwarmHandler, Swarm>()
                .Build();

            var parsed = Parser.Default.ParseArguments<RestartApplication, RestartPool, GetInstance, Swarm>(args);
            await parsed.MapResult(
                async (RestartApplication opts) => await pools.ExecuteAsync(opts, cancellation.Token),
                async (RestartPool opts) => await pools.ExecuteAsync(opts, cancellation.Token),
                async (GetInstance opts) => await pools.ExecuteAsync(opts, cancellation.Token),
                async (Swarm opts) => await pools.ExecuteAsync(opts, cancellation.Token),
                err => Task.FromResult(-1));
        }
    }
}