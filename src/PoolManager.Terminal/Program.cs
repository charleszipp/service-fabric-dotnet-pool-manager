using CommandLine;
using Microsoft.ServiceFabric.Actors.Client;
using Ninject;
using PoolManager.Core.Builders;
using PoolManager.Core.Resolvers;
using PoolManager.SDK.Pools;
using PoolManager.Terminal.Commands;
using System.Fabric;
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
            
            var mediator = new MediatorBuilder(resolver)
                .WithCommandHandler<RestartApplicationHandler, RestartApplication>()
                .WithCommandHandler<RestartPoolHandler, RestartPool>()
                .WithCommandHandler<GetInstanceHandler, GetInstance>()
                .WithCommandHandler<SwarmHandler, Swarm>()
                .WithCommandHandler<EnsureAppReadyHandler, EnsureAppReady>()
                .Build();

            var parsed = Parser.Default.ParseArguments<RestartApplication, RestartPool, GetInstance, Swarm, EnsureAppReady>(args);
            await parsed.MapResult(
                async (RestartApplication opts) => await mediator.ExecuteAsync(opts, cancellation.Token),
                async (RestartPool opts) => await mediator.ExecuteAsync(opts, cancellation.Token),
                async (GetInstance opts) => await mediator.ExecuteAsync(opts, cancellation.Token),
                async (Swarm opts) => await mediator.ExecuteAsync(opts, cancellation.Token),
                async (EnsureAppReady opts) => await mediator.ExecuteAsync(opts, cancellation.Token),
                err => Task.FromResult(-1));
        }
    }
}