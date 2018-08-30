using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoolManager.Pools;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using ServiceFabric.Mocks;

namespace PoolManager.UnitTests
{
    public interface IMyStatefulActor : IActor
    {
        Task InsertAsync(string stateName, Payload value);
    }
    [DataContract]
    public class Payload
    {
        [DataMember]
        public readonly string Content;
        public Payload(string content)
        {
            Content = content;
        }
    }
    [StatePersistence(StatePersistence.Persisted)]
    public class MyStatefulActor : Actor, IMyStatefulActor
    {
        public MyStatefulActor(ActorService actorService, ActorId actorId) : base(actorService, actorId) { }
        public async Task InsertAsync(string stateName, Payload value)
        {
            await StateManager.AddStateAsync(stateName, value);
        }
    }
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
        }
        private Pool CreateActor(ActorId id)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new Pool(service, id, null);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<Pool>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}