using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var id = new ActorId(actorGuid);
            var actor = CreateActor(id);
            var stateManager = (MockActorStateManager)actor.StateManager;

            const string stateName = "test";
            var payload = new Payload(StatePayload);
            await actor.InsertAsync(stateName, payload);

            var actual = await stateManager.GetStateAsync<Payload>(stateName);
            actual.Content.Should().Be(StatePayload);
        }
        private MyStatefulActor CreateActor(ActorId id)
        {
            ActorBase ActorFactory(ActorService service, ActorId actorId) => new MyStatefulActor(service, id);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<MyStatefulActor>(ActorFactory);
            var actor = svc.Activate(id);
            return actor;
        }
    }
}