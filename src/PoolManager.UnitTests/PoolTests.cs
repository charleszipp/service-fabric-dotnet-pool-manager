using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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
using PartitionSchemeDescription = PoolManager.SDK.PartitionSchemeDescription;

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
        public static TestContext WithHappyPath()
        {
            var testContext = new TestContext();
            testContext.TelemetryClient = new TelemetryClient(new TelemetryConfiguration("", testContext.MockTelemetryChannel));
            testContext.Actors = Enumerable.Range(0, 10).Select(x => Guid.NewGuid()).Select(x =>
            {
                var actorProxyFactory = new MockActorProxyFactory();
                var mockServiceProxyFactory = new MockServiceProxyFactory();
                var actorServiceForActor = MockActorServiceFactory.CreateActorServiceForActor<Instance>();
                var instance = new Instance(actorServiceForActor,
                    new ActorId(x), testContext.ClusterClient.Object, testContext.TelemetryClient, actorProxyFactory,
                    mockServiceProxyFactory);
                return instance;
            }).ToList();

            var setupSequentialResult = testContext.GuidGetter.SetupSequence(x => x.GetAGuid());
            foreach (var actor in testContext.Actors)
            {
                setupSequentialResult.Returns(actor.Id.GetGuidId());
                testContext.MockActorProxyFactory.RegisterActor(actor);
            }
            testContext.PoolActorService = CreatePoolActorService(testContext.TelemetryClient, testContext.MockActorProxyFactory,
                testContext.GuidGetter.Object);
            testContext.Pool = testContext.PoolActorService.Activate(new ActorId("fabric:/myapplicationname/myservicetypename"));
            return testContext;
        }
        private static MockActorService<Pool> CreatePoolActorService(TelemetryClient telemetryClient, IActorProxyFactory actorProxyFactory,
            IGuidGetter guidGetter)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Pool>((svc, id) =>
                new Pool(svc, id, telemetryClient, new InstanceProxy(actorProxyFactory, guidGetter)));
        }
    }
    [TestClass]
    public class StartingAPool
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest());
        }
        [TestMethod]
        public void TracksBeginning()
        {
            _testContext.MockTelemetryChannel.SentTelemetry.Should().Contain(x =>
                x is TraceTelemetry && ((TraceTelemetry)x).Message == "pools.vacant.grow.block.time.Began");
        }
        [TestMethod]
        public void TracksCompletionWithDuration()
        {
            _testContext.MockTelemetryChannel.SentTelemetry.Should().Contain(x =>
                x is TraceTelemetry && ((TraceTelemetry)x).Message == "pools.vacant.grow.block.time.Completed" && ((TraceTelemetry)x).Properties.ContainsKey("duration"));
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
    }
    [TestClass]
    public class StartingAPoolWithIdlePoolSizeOfThree
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(idleServicesPoolSize:3));
        }
        [TestMethod]
        public void CreatesThreeStatefulServices()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(3));
        }
    }
    [TestClass]
    public class StartingAPoolWithHasPersistedStateOfFalse
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(hasPersistedState:false));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithHasPersistedStateOfTrue()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), It.IsAny<int>(), false), Times.Exactly(10));
        }
    }
    [TestClass]
    public class StartingAPoolWithMinReplicasOfSix
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(minReplicas:6));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithMinReplicasOfSix()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), 6, It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
    }
    [TestClass]
    public class StartingAPoolWithTargetReplicasOfTwo
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(targetReplicas:2));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithTargetReplicasOfTwo()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), 2, It.IsAny<bool>()), Times.Exactly(10));
        }
    }
    [TestClass]
    public class StartingAPoolWithPartitionSchemeDescriptionOfNamed
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(partitionScheme:PartitionSchemeDescription.Named));
        }
        [TestMethod]
        public void CreatesTenStatefulServicesWithPartitionSchemeDescriptionOfNamed()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y=>y.PartitionSchemeDescription is NamedPartitionSchemeDescription), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
    }
    [TestClass]
    public class StartingAPoolWithMaxPoolSizeOfTwo
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(maxPoolSize:2));
        }
        [TestMethod]
        public void CreatesTwoStatefulServices()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(2));
        }
    }
    [TestClass]
    public class StartingAPoolWithServicesAllocationBlockSizeOfTwo
    {
        private TestContext _testContext;
        [TestInitialize]
        public async Task Setup()
        {
            _testContext = TestContext.WithHappyPath();

            // Act
            await _testContext.Pool.StartAsync(new StartPoolRequest(servicesAllocationBlockSize:2));
        }
        [TestMethod]
        public void CreatesTenStatefulServices()
        {
            _testContext.ClusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.IsAny<ServiceDescriptionFactory>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(10));
        }
        [TestMethod]
        public void LogsEachAllocationBlockIndividually()
        {
            _testContext.MockTelemetryChannel.SentTelemetry.Count(x =>
                x is TraceTelemetry t && t.Message == "pools.vacant.grow.block.time.Completed" && t.Properties.ContainsKey("duration"))
                .Should().Be(5);
        }
    }
}