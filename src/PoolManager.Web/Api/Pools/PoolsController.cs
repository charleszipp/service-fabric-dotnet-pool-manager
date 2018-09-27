using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PoolManager.SDK.Pools;

namespace PoolManager.Web.Api.Pools
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoolsController : ControllerBase
    {
        private readonly IPoolProxy pools;

        public PoolsController(IPoolProxy pools)
        {
            this.pools = pools;
        }

        [HttpGet]
        public async Task<ActionResult<Pool>> Get([Required]string serviceTypeUri)
        {
            var configuration = await pools.GetConfigurationAsync(serviceTypeUri);
            if(configuration == null)
                return NotFound($"Pool {serviceTypeUri} does not exist or has not yet been started.");

            var getInstancesResponse = await pools.GetInstancesAsync(serviceTypeUri, CancellationToken.None);
            
            var response = new Pool(
                new PoolConfiguration(configuration.ExpirationQuanta, configuration.HasPersistedState, configuration.IdleServicesPoolSize,
                    configuration.IsServiceStateful, configuration.MaxPoolSize, configuration.MinReplicaSetSize,
                    (PartitionSchemeDescription)Enum.Parse(typeof(PartitionSchemeDescription), configuration.PartitionScheme.ToString()),
                    configuration.ServicesAllocationBlockSize, configuration.TargetReplicasetSize),
                getInstancesResponse.VacantInstances,
                getInstancesResponse.OccupiedInstances
            );

            return Ok(response);
        }
    }
}
