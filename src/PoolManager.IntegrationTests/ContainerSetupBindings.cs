using BoDi;
using Microsoft.ServiceFabric.Actors.Client;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Pools;
using System.Fabric;
using TechTalk.SpecFlow;

namespace PoolManager.IntegrationTests
{
    [Binding]
    public class ContainerSetupBindings
    {
        private readonly IObjectContainer _container;

        public ContainerSetupBindings(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupContainer()
        {
            _container.RegisterInstanceAs(new FabricClient());
            _container.RegisterInstanceAs<IActorProxyFactory>(new ActorProxyFactory());
            _container.RegisterInstanceAs<IPartitionProxy>(
                new PartitionProxy(
                    _container.Resolve<IActorProxyFactory>(),
                    _container.Resolve<FabricClient>()
                    )
                );
            _container.RegisterInstanceAs<IPoolProxy>(
                new PoolProxy(
                    _container.Resolve<IActorProxyFactory>(),
                    _container.Resolve<IPartitionProxy>(),
                    _container.Resolve<FabricClient>()
                    )
                );
        }
    }
}