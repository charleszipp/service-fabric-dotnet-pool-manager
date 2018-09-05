using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PoolManager.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools.Requests;
using ServiceFabric.Mocks;

namespace PoolManager.UnitTests
{
    [TestClass]
    public class PoolTests
    {
        private Mock<IActorProxyFactory> _instanceProxyFactory;
        private Mock<IInstance> _instance;
        private MockActorService<Pool> _poolActorService;
        private Pool _sut;
        [TestInitialize]
        public async Task StartingAPool()
        {
            _instance = new Mock<IInstance>();
            _instanceProxyFactory = new Mock<IActorProxyFactory>();
            _instanceProxyFactory.Setup(x => x.CreateActorProxy<IInstance>(It.IsAny<ActorId>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_instance.Object);
            _poolActorService = CreatePoolActorService(new TelemetryClient(), _instanceProxyFactory.Object);
            _sut = _poolActorService.Activate(new ActorId("someId"));
            
            // Act
            await _sut.StartAsync(new StartPoolRequest());
        }
        [TestMethod]
        public void StartsInstances()
        {
            // Assert
            _instance.Verify(x => x.StartAsync(It.IsAny<StartInstanceRequest>()));
        }
        private static MockActorService<Pool> CreatePoolActorService(TelemetryClient telemetryClient, IActorProxyFactory actorProxyFactory)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Pool>(
                (svc, id) => new Pool(svc, id, telemetryClient, actorProxyFactory));
        }
    }
}