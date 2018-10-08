using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Partitions
{
    public class OccupyInstanceProxy : IHandleCommand<OccupyInstance, OccupyInstanceResult>
    {
        private readonly IInstanceProxy instances;

        public OccupyInstanceProxy(IInstanceProxy instances)
        {
            this.instances = instances;
        }

        public async Task<OccupyInstanceResult> ExecuteAsync(OccupyInstance command, CancellationToken cancellationToken)
        {
            var response = await instances.OccupyAsync(command.InstanceId, new OccupyRequest(command.PartitionId, command.InstanceName));
            return new OccupyInstanceResult(response.ServiceName);
        }
    }
}