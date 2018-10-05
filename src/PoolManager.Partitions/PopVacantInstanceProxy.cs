using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Pools;
using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Partitions
{
    public class PopVacantInstanceProxy : IHandleCommand<PopVacantInstance, PopVacantInstanceResult>
    {
        private readonly IPoolProxy pools;

        public PopVacantInstanceProxy(IPoolProxy pools)
        {
            this.pools = pools;
        }

        public async Task<PopVacantInstanceResult> ExecuteAsync(PopVacantInstance command, CancellationToken cancellationToken)
        {
            var response = await pools.PopVacantInstanceAsync(command.ServiceTypeUri, new PopVacantInstanceRequest());
            return new PopVacantInstanceResult(response.InstanceId);
        }
    }
}
