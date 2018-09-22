using PoolManager.Core;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class DeleteServiceHandler : IHandleCommand<DeleteService>
    {
        private readonly IClusterClient cluster;

        public DeleteServiceHandler(IClusterClient cluster) =>
            this.cluster = cluster;

        public Task ExecuteAsync(DeleteService command, CancellationToken cancellationToken) =>
            cluster.DeleteServiceAsync(command.ServiceName);
    }
}