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
    [TestClass]
    public class PoolTests
    {
        private Mock<IClusterClient> _clusterClient;
        private MockActorService<Pool> _poolActorService;
        private Pool _sut;
        [TestInitialize]
        public async Task StartingAPool()
        {
            var mockActorProxyFactory = new MockActorProxyFactory();
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration("", new MockTelemetryChannel()));
            var guidGetter = new Mock<IGuidGetter>();
            var actors = Enumerable.Range(0, 10).Select(x => Guid.NewGuid()).Select(x =>
            {
                var actorProxyFactory = new MockActorProxyFactory();
                var mockServiceProxyFactory = new MockServiceProxyFactory();
                var actorServiceForActor = MockActorServiceFactory.CreateActorServiceForActor<Instance>();
                var instance = new Instance(actorServiceForActor,
                    new ActorId(x), _clusterClient.Object, telemetryClient, actorProxyFactory,
                    mockServiceProxyFactory);
                return instance;
            });
            
            _clusterClient = new Mock<IClusterClient>();
            var setupSequentialResult = guidGetter.SetupSequence(x => x.GetAGuid());
            foreach (var actor in actors)
            {
                setupSequentialResult.Returns(actor.Id.GetGuidId());
                mockActorProxyFactory.RegisterActor(actor);
            }
            _poolActorService = CreatePoolActorService(telemetryClient, mockActorProxyFactory, guidGetter.Object);
            _sut = _poolActorService.Activate(new ActorId("fabric:/myapplicationname/myservicetypename"));

            // Act
            await _sut.StartAsync(new StartPoolRequest());
        }
        [TestMethod]
        public void StartsInstances()
        {
            // Assert
            _clusterClient.Verify(x => x.CreateStatefulServiceAsync(
                It.Is<ServiceDescriptionFactory>(y =>
                    y.ServiceTypeName == "myservicetypename" && y.PartitionSchemeDescription.Scheme == PartitionScheme.UniformInt64Range
                    && y.ApplicationName.AbsoluteUri == "fabric:/myapplicationname"
                    && y.ServiceName.AbsoluteUri.StartsWith("fabric:/myapplicationname/")
                ), 1, 3, true), Times.Exactly(10));
        }
        private static MockActorService<Pool> CreatePoolActorService(TelemetryClient telemetryClient, IActorProxyFactory actorProxyFactory,
            IGuidGetter guidGetter)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Pool>((svc, id) =>
                new Pool(svc, id, telemetryClient, new InstanceProxy(actorProxyFactory, guidGetter)));
        }
    }
}