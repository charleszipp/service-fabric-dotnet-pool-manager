using BoDi;
using Microsoft.ServiceFabric.Actors.Client;
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
            _container.RegisterInstanceAs<IPoolProxy>(new PoolProxy(new ActorProxyFactory()));
        }
    }
}