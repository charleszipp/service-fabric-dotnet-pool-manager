using System;
using System.Fabric.Description;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PoolManager.Core;
using PoolManager.Instances;
using PoolManager.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using ServiceFabric.Mocks;

namespace PoolManager.UnitTests
{
    [TestClass]
    public class InstanceTests
    {
        private Mock<IActorProxyFactory> _instanceProxyFactory;
        private Mock<IInstance> _instance;
        private TelemetryClient _telemetryClient;
        private ActorId _actorId;
        private Pool _sut;
        private Mock<IClusterClient> _clusterClient;
        [TestInitialize]
        public async Task StartingAnInstance()
        {
            _instanceProxyFactory = new Mock<IActorProxyFactory>();
            _instance = new Mock<IInstance>();
            _instanceProxyFactory.Setup(x =>
                    x.CreateActorProxy<IInstance>(It.IsAny<ActorId>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_instance.Object);
            _telemetryClient = new TelemetryClient();
            _actorId = new ActorId(Guid.NewGuid());
            
            _clusterClient = new Mock<IClusterClient>();
            var instanceActorService = CreateInstanceActorService(_clusterClient.Object, _telemetryClient);
            var instanceActor = instanceActorService.Activate(_actorId);
            await instanceActor.StartAsync(new StartInstanceRequest("fabric:/ServicePoolManagerLoadTestHarness/NoOpType"));
        }
        [TestMethod]
        public void CreatesAStatefulService()
        {
            _clusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y =>
                    y.ServiceTypeName == "NoOpType" && y.PartitionSchemeDescription.Scheme == PartitionScheme.UniformInt64Range
                    && y.ApplicationName.AbsoluteUri == "fabric:/ServicePoolManagerLoadTestHarness"
                    && y.ServiceName.AbsoluteUri.StartsWith("fabric:/ServicePoolManagerLoadTestHarness/")
                ), 1, 3, true));
        }
        private static MockActorService<Instance> CreateInstanceActorService(IClusterClient cluster, TelemetryClient telemetryClient)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Instance>(
                (svc, id) => new Instance(svc, id, cluster, telemetryClient, new MockActorProxyFactory(), new MockServiceProxyFactory()));
        }
    }
}