using Microsoft.ApplicationInsights;
using PoolManager.Core.ApplicationInsights;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Pools.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Pools
{
    public class EnsurePoolSizeHandler : IHandleCommand<EnsurePoolSize>
    {
        private readonly IPoolsRepository repository;
        private readonly TelemetryClient telemetryClient;
        private readonly IHandleCommand<PushVacantInstance> addVacantInstance;

        public EnsurePoolSizeHandler(IPoolsRepository repository, TelemetryClient telemetryClient, IHandleCommand<PushVacantInstance> addVacantInstance)
        {
            this.repository = repository;
            this.telemetryClient = telemetryClient;
            this.addVacantInstance = addVacantInstance;
        }

        public async Task ExecuteAsync(EnsurePoolSize command, CancellationToken cancellationToken)
        {
            var vacantInstanceTarget = await repository.GetVacantInstanceTargetAsync(cancellationToken);
            var vacantInstanceCount = await repository.GetVacantInstanceCountAsync(cancellationToken);

            var vacantInstanceDeficit = vacantInstanceTarget - vacantInstanceCount;

            telemetryClient.GetMetric("pools.vacant.count").TrackValue(vacantInstanceCount);
            telemetryClient.GetMetric("pools.vacant.target").TrackValue(vacantInstanceTarget);
            telemetryClient.GetMetric("pools.vacant.deficit").TrackValue(vacantInstanceDeficit);

            if (vacantInstanceDeficit == 0)
                return;

            while (vacantInstanceDeficit > 0)
            {
                var allocationBlockSize = await repository.GetAllocationBlockSizeAsync(cancellationToken);
                telemetryClient.GetMetric("pools.vacant.block.size").TrackValue(vacantInstanceTarget);
                using (telemetryClient.TrackMetricTimer("pools.vacant.grow.block.time"))
                {
                    Task[] addTasks = new Task[Math.Min(allocationBlockSize, vacantInstanceDeficit)];
                    for (var i = 0; i < addTasks.Length; i++)
                    {
                        addTasks[i] = addVacantInstance.ExecuteAsync(new PushVacantInstance(), cancellationToken);
                        vacantInstanceDeficit--;
                    }

                    await Task.WhenAll(addTasks);
                }
            }
        }
    }
}
