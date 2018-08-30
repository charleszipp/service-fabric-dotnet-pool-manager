using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

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
            var response = await _pools.GetConfigurationAsync(serviceTypeUri);
            response.ServiceTypeUri.Should().Be(serviceTypeUri);
        }

        [Then(@"there should be ""(.*)"" service instances for service fabric application ""(.*)"" and service type ""(.*)""")]
        public async Task ThenThereShouldBeServiceInstancesForServiceFabricApplicationAndServiceType(int instances, string applicationName, string serviceTypeName)
        {
            ScenarioContext.Current.Pending();
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
}
