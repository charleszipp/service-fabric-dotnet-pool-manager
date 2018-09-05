using FluentAssertions;
using Microsoft.ServiceFabric.Actors;
using Moq;
using PoolManager.Core.Tests;
using PoolManager.Pools;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using ServiceFabric.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace PoolManager.UnitTests
{
    [Binding]
    public class PoolsBindings
    {
        private readonly MockActorService<Pool> _poolActorService;
        private readonly Mock<IInstanceProxy> _instanceProxyMock;
        private Pool _pool;
        private readonly List<StartInstanceRequest> _instancesStarted = new List<StartInstanceRequest>();

        public PoolsBindings(MockActorService<Pool> poolActorService, Mock<IInstanceProxy> instanceProxyMock)
        {
            _poolActorService = poolActorService;
            _instanceProxyMock = instanceProxyMock;
        }

        [Before]
        public void Before()
        {
            _instancesStarted.Clear();
            _instanceProxyMock.Setup(i => i.StartAsync(It.IsAny<StartInstanceRequest>()))
                .Callback<StartInstanceRequest>(startRequest => _instancesStarted.Add(startRequest));
        }

        [Given(@"the idle pool ""(.*)""")]
        public void GivenTheIdlePool(string poolId)
        {            
            _pool = _poolActorService.Activate(new ActorId(poolId));
        }

        [When(@"the pool ""(.*)"" is started with the following configuration")]
        public async Task WhenThePoolIsStartedWithTheFollowingConfiguration(string poolId, Table table)
        {
            var request = table.CreateImmutableInstance<StartPoolRequest>();
            await _pool.StartAsync(request);
        }

        [Then(@"the ""(.*)"" pool configuration should be")]
        public async Task ThenThePoolConfigurationShouldBe(string poolId, Table table)
        {
            var expected = table.CreateImmutableInstance<ConfigurationResponse>();
            var response = await _pool.GetConfigurationAsync();
            response.ExpirationQuanta.Should().Be(expected.ExpirationQuanta);
            response.HasPersistedState.Should().Be(expected.HasPersistedState);
            response.IdleServicesPoolSize.Should().Be(expected.IdleServicesPoolSize);
            response.IsServiceStateful.Should().Be(expected.IsServiceStateful);
            response.MaxPoolSize.Should().Be(expected.MaxPoolSize);
            response.MinReplicaSetSize.Should().Be(expected.MinReplicaSetSize);
            response.PartitionScheme.Should().Be(expected.PartitionScheme);
            response.ServicesAllocationBlockSize.Should().Be(expected.ServicesAllocationBlockSize);
            response.ServiceTypeUri.Should().Be(expected.ServiceTypeUri);
            response.TargetReplicasetSize.Should().Be(expected.TargetReplicasetSize);
        }

        [Then(@"there should be ""(.*)"" service instances for service fabric application ""(.*)"" and service type ""(.*)""")]
        public void ThenThereShouldBeServiceInstancesForServiceFabricApplicationAndServiceType(int instancesCount, string applicationName, string serviceTypeName)
        {
            _instancesStarted.Should().HaveCount(instancesCount);
        }
    }
}
