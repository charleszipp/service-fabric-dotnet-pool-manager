using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Responses;

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

        [HttpGet("{serviceTypeUri}", Name = "GetPool")]
        public async Task<ActionResult<Pool>> Get(string serviceTypeUri)
        {
            ConfigurationResponse cfg = null;
            try
            {
                cfg = await pools.GetConfigurationAsync(serviceTypeUri);
            }
            catch(InvalidOperationException ex) when (ex.Message.ToLower().Contains("no configuration"))
            {
                return NotFound($"Pool {serviceTypeUri} does not exist or has not yet been started.");
            }
            
            var getVacantInstancesResult = await pools.GetVacantInstancesAsync(serviceTypeUri);
            
            var response = new Pool(
                new PoolConfiguration(cfg.ExpirationQuanta, cfg.HasPersistedState, cfg.IdleServicesPoolSize,
                    cfg.IsServiceStateful, cfg.MaxPoolSize, cfg.MinReplicaSetSize,
                    (PartitionSchemeDescription)Enum.Parse(typeof(PartitionSchemeDescription), cfg.PartitionScheme.ToString()),
                    cfg.ServicesAllocationBlockSize, cfg.TargetReplicasetSize),
                getVacantInstancesResult.VacantInstances
            );

            return Ok(response);
        }
    }
}
