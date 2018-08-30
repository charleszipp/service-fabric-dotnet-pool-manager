using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoolManager.Instances;
using PoolManager.Pools;
using PoolManager.SDK.Instances.Requests;
using ServiceFabric.Mocks;

namespace PoolManager.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private const string StatePayload = "some value";
        [TestMethod]
        public async Task TestMethod1()
        {
            var actorGuid = Guid.NewGuid();
            var id = new ActorId("fabric:/ServicePoolManagerLoadTestHarness/NoOpType");
            var actor = CreateActor(id);
            var stateManager = (MockActorStateManager)actor.StateManager;

            // TODO Requires mocking FabricClient
            // await actor.StartAsync(new StartInstanceRequest("fabric:/ServicePoolManagerLoadTestHarness/NoOpType"));
        }
        private Instance CreateActor(ActorId id)
        {
            return MockActorServiceFactory.CreateActorServiceForActor<Instance>(ActorFactory).Activate(id);
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new Instance(service, id, null);
        }
    }
}