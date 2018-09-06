using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PoolManager.Core;
using PoolManager.Instances;
using PoolManager.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Pools.Requests;
using PoolManager.UnitTests.Mocks;
using ServiceFabric.Mocks;

namespace PoolManager.UnitTests
{
    public class TestContext
    {
        public IList<Instance> Actors { get; set; }
        public Mock<IClusterClient> ClusterClient { get; set; } = new Mock<IClusterClient>();
        public Mock<IGuidGetter> GuidGetter { get; set; } = new Mock<IGuidGetter>();
        public MockActorProxyFactory MockActorProxyFactory { get; set; } = new MockActorProxyFactory();
        public MockTelemetryChannel MockTelemetryChannel { get; set; } = new MockTelemetryChannel();
        public Pool Pool { get; set; }
        public MockActorService<Pool> PoolActorService { get; set; }
        public TelemetryClient TelemetryClient { get; set; }
    }
    [TestClass]
    public class StartingAPool
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = new TestContext();
            _testContext.TelemetryClient = new TelemetryClient(new TelemetryConfiguration("", _testContext.MockTelemetryChannel));
            _testContext.Actors = Enumerable.Range(0, 10).Select(x => Guid.NewGuid()).Select(x =>
            {
                var actorProxyFactory = new MockActorProxyFactory();
                var mockServiceProxyFactory = new MockServiceProxyFactory();
                var actorServiceForActor = MockActorServiceFactory.CreateActorServiceForActor<Instance>();
                var instance = new Instance(actorServiceForActor,
                    new ActorId(x), _testContext.ClusterClient.Object, _testContext.TelemetryClient, actorProxyFactory,
                    mockServiceProxyFactory);
                return instance;
            }).ToList();

            var setupSequentialResult = _testContext.GuidGetter.SetupSequence(x => x.GetAGuid());
            foreach (var actor in _testContext.Actors)
            {
                setupSequentialResult.Returns(actor.Id.GetGuidId());
                _testContext.MockActorProxyFactory.RegisterActor(actor);
            }
            _testContext.PoolActorService = CreatePoolActorService(_testContext.TelemetryClient, _testContext.MockActorProxyFactory,
                _testContext.GuidGetter.Object);
            _testContext.Pool = _testContext.PoolActorService.Activate(new ActorId("fabric:/myapplicationname/myservicetypename"));

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest());
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithMinReplicasOfOne()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), 1, It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithTargetReplicasOfThree()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), 3, It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithHasPersistedStateSetToTrue()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), It.IsAny<int>(), true), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesOfSpecifiedType()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y => y.ServiceTypeName == "myservicetypename"),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithUniformInt64RangePartitionScheme()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y => y.PartitionSchemeDescription.Scheme == PartitionScheme.UniformInt64Range),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesInSpecifiedApplication()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y => y.ApplicationName.AbsoluteUri == "fabric:/myapplicationname"),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithNameDerivedFromApplicationAndActorId()
        {
            foreach (var actor in _testContext.Actors)
                _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                    It.Is<ServiceDescriptionFactory>(y =>
                        y.ServiceName.AbsoluteUri == "fabric:/myapplicationname/myservicetypename/" + actor.Id.GetGuidId()),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(1));
        }
        private static MockActorService<Pool> CreatePoolActorService(TelemetryClient telemetryClient, IActorProxyFactory actorProxyFactory,
            IGuidGetter guidGetter)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Pool>((svc, id) =>
                new Pool(svc, id, telemetryClient, new InstanceProxy(actorProxyFactory, guidGetter)));
        }
    }
}