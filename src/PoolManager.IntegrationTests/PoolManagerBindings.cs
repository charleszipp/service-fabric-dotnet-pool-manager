using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PoolManager.SDK.Pools.Responses;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Service = System.Fabric.Query.Service;

namespace PoolManager.IntegrationTests
{
    [Binding]
    public class PoolManagerBindings
    {
        private readonly FabricClient _fabricClient;
        private readonly IPoolProxy _pools;
        private const string _applicationTypeVersion = "1.0.0";
        private const string _serviceTypeVersion = "1.0.0";

        public PoolManagerBindings(FabricClient fabricClient, IPoolProxy pools)
        {
            _fabricClient = fabricClient;
            _pools = pools;
        }        

        [Given(@"the service fabric application name ""(.*)"" for type ""(.*)"" exists with no services")]
        public async Task GivenTheServiceFabricApplicationInstanceExists(string applicationName, string applicationTypeName)
        {
            var types = await _fabricClient.QueryManager.GetApplicationTypeListAsync(applicationTypeName);
            if (types?.Any() ?? false)
            {
                var applicationUri = new Uri(applicationName);
                var applications = await _fabricClient.QueryManager.GetApplicationListAsync(applicationUri);
                if (applications?.Any() ?? false)
                {
                    DeleteApplicationDescription delete = new DeleteApplicationDescription(applicationUri);
                    await _fabricClient.ApplicationManager.DeleteApplicationAsync(delete);
                }
                
                ApplicationDescription applicationDescription = new ApplicationDescription(applicationUri, applicationTypeName, _applicationTypeVersion);
                await _fabricClient.ApplicationManager.CreateApplicationAsync(applicationDescription);
            }
            else
                throw new ArgumentException($"Application type '{applicationTypeName}' not deployed to the cluster.");
        }

        [Given(@"the service fabric application type ""(.*)"" has ""(.*)"" service type")]
        public async Task GivenTheServiceFabricApplicationInstanceHasServiceType(string applicationTypeName, string serviceTypeName)
        {
            var types = await _fabricClient.QueryManager.GetServiceTypeListAsync(applicationTypeName, _applicationTypeVersion);
            if(!types?.Any() ?? false)
                throw new ArgumentException($"Service type '{serviceTypeName}' not found for application type '{applicationTypeName}'");
        }

        [Given(@"the service fabric application ""(.*)"" has no service instances for ""(.*)"" service type")]
        public async Task GivenTheServiceFabricApplicationTypeHasNoServiceInstancesForServiceType(string applicationName, string serviceTypeName)
        {
            var applicationUri = new Uri(applicationName);
            var serviceList = await _fabricClient.QueryManager.GetServiceListAsync(applicationUri);
            var services = serviceList?.Where(service => service.ServiceTypeName == serviceTypeName);
            if(services?.Any() ?? false)
            {
                var deletes = services
                    .Select(service => new DeleteServiceDescription(service.ServiceName))
                    .Select(delete => _fabricClient.ServiceManager.DeleteServiceAsync(delete));
                await Task.WhenAll(deletes);
            }
        }

        [Given(@"the service pool ""(.*)"" does not exist")]
        public async Task GivenTheServicePoolDoesNotExist(string serviceTypeUri)
        {
            await _pools.DeletePoolAsync(serviceTypeUri);
        }


        [When(@"the ""(.*)"" pool is started with the following configuration")]
        public async Task WhenThePoolIsStartedWithTheFollowingConfiguration(string serviceTypeUri, Table table)
        {
            var request = table.CreateImmutableInstance<StartPoolRequest>();
            await _pools.StartPoolAsync(serviceTypeUri, request);
        }
        [Then(@"the ""(.*)"" pool configuration should be")]
        public async Task ThenThePoolConfigurationShouldBe(string serviceTypeUri, Table table)
        {
            var expected = table.CreateImmutableInstance<ConfigurationResponse>();
            var response = await _pools.GetConfigurationAsync(serviceTypeUri);
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
        public async Task ThenThereShouldBeServiceInstancesForServiceFabricApplicationAndServiceType(int instanceCount, string applicationName, string serviceTypeName)
        {
            var serviceList = await _fabricClient.QueryManager.GetServiceListAsync(new Uri(applicationName));
            serviceList.Count(service => service.ServiceTypeName == serviceTypeName).Should().Be(instanceCount);
        }
        [When(@"one of the ""(.*)"" services moves")]
        public async Task WhenOneOfTheServicesMoves(string fullyQualifiedServiceTypeName)
        {
            var applicationName = fullyQualifiedServiceTypeName.ParseServiceTypeUri().ApplicationName;
            var serviceTypeName = fullyQualifiedServiceTypeName.ParseServiceTypeUri().ServiceTypeName;
            var service = (await _fabricClient.QueryManager.GetServiceListAsync(new Uri(applicationName)))
                // TODO Pull back the first healthy one, instead of first overall and check health?
                .First(x => x.ServiceTypeName == serviceTypeName);

            var task = WaitForAnyServiceToBecomeHealthy();
            if (await Task.WhenAny(task, Task.Delay(20000)) != task)
                throw new Exception($"Gave up waiting for {service.ServiceName} to become healthy");
            await _fabricClient.FaultManager.MovePrimaryAsync(PartitionSelector.SingletonOf(service.ServiceName));
            async Task WaitForAnyServiceToBecomeHealthy()
            {
                while (service.HealthState != HealthState.Ok)
                {
                    service = (await _fabricClient.QueryManager.GetServiceListAsync(new Uri(applicationName)))
                        .First(x => x.ServiceTypeName == serviceTypeName);
                    await Task.Delay(1000);
                }
            }
        }
        [Then(@"each service fabric application ""(.*)"" and service type ""(.*)"" instance should have the following configuration")]
        public async Task ThenEachServiceFabricApplicationAndServiceTypeInstanceShouldHaveTheFollowingConfiguration(string applicationName, string serviceTypeName, Table configurationTable)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"each service fabric application ""(.*)"" and service type ""(.*)"" instance partition should be healthy")]
        public async Task ThenEachServiceFabricApplicationAndServiceTypeInstancePartitionShouldBeHealthy(string applicationName, string serviceTypeName)
        {
            ScenarioContext.Current.Pending();
        }
    }
    public static class ServiceTypeUriStringExtensions
    {
        internal static (string ApplicationName, string ServiceTypeName) ParseServiceTypeUri(this string serviceTypeUri)
        {
            var indexLastSlash = serviceTypeUri.LastIndexOf('/');
            string applicationName;
            string serviceTypeName;
            if (indexLastSlash >= 0)
            {
                applicationName = serviceTypeUri.Substring(0, indexLastSlash);
                serviceTypeName = serviceTypeUri.Substring(indexLastSlash + 1);
            }
            else
            {
                applicationName = null;
                serviceTypeName = null;
            }
            return (applicationName, serviceTypeName);
        }
    }
}
