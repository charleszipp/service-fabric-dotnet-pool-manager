using System;
using BoDi;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Moq;
using PoolManager.Core;
using PoolManager.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools;
using PoolManager.UnitTests.Mocks;
using ServiceFabric.Mocks;
using TechTalk.SpecFlow;

namespace PoolManager.UnitTests
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
            _container.RegisterInstanceAs(new MockTelemetryChannel());
            _container.RegisterFactoryAs(container =>
                new TelemetryClient(new TelemetryConfiguration(Guid.NewGuid().ToString(), container.Resolve<MockTelemetryChannel>())));
            _container.RegisterInstanceAs(new Mock<IClusterClient>());
            _container.RegisterFactoryAs(c => c.Resolve<Mock<IClusterClient>>().Object);
            _container.RegisterInstanceAs(new Mock<IPoolProxy>());
            _container.RegisterFactoryAs(c => c.Resolve<Mock<IPoolProxy>>().Object);
            _container.RegisterInstanceAs<IServiceProxyFactory>(new MockServiceProxyFactory());
            _container.RegisterFactoryAs(container => MockActorServiceFactory.CreateActorServiceForActor<Pool>((svc, id) =>
                new Pool(svc, id)));
        }
    }
}