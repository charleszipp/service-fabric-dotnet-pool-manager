using System.Fabric;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Client;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using PoolManager.Monitor.Interfaces;
using PoolManager.Monitor.Orphans;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Pools;

namespace PoolManager.Monitor.Modules
{
    public class ServiceModule : NinjectModule
    {
        private readonly StatelessServiceContext _context;

        public ServiceModule(StatelessServiceContext context)
        {
            _context = context;
        }
        public override void Load()
        {
            Bind<TelemetryClient>().ToSelf();
            Bind<FabricClient>().ToSelf();
            Bind<IRemoveOrphanCommandFactory>().ToFactory();
            Bind<PoolManagerMonitor>().ToSelf().InSingletonScope();
            Bind<IPoolProxy>().To<PoolProxy>().InSingletonScope();
            Bind<IPartitionProxy>().To<PartitionProxy>().InSingletonScope();

            Bind<StatelessServiceContext>().ToConstant(_context).InSingletonScope();
            Bind<PoolManagerMonitorService>().ToSelf();
            Bind<ServiceContext>().ToConstant(_context).InSingletonScope();
            Bind<IActorProxyFactory>().ToMethod(ctx =>
                new CorrelatingActorProxyFactory(
                    ctx.Kernel.Get<StatelessServiceContext>(),
                    callbackClient => new FabricTransportActorRemotingClientFactory(callbackClient)
                )
            ).InSingletonScope();
            Bind<IMonitor>().To<OrphanMonitor>().Named("OrphanMonitor");
            Bind<RemoveOrphanCommand>().ToSelf();
        }
    }
}