using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances.States
{
    public abstract class InstanceState
    {
        public abstract InstanceStates State { get; }

        public abstract Task<InstanceState> StartAsync(InstanceContext context, StartInstance command, CancellationToken cancellationToken);

        public abstract Task<InstanceState> OccupyAsync(InstanceContext context, OccupyInstance command, CancellationToken cancellationToken);

        public abstract Task<InstanceState> VacateAsync(InstanceContext context, VacateInstance command, CancellationToken cancellationToken);

        public abstract Task<InstanceState> RemoveAsync(InstanceContext context, RemoveInstance command, CancellationToken cancellationToken);

        public abstract Task<ReportActivityResult> ReportActivityAsync(InstanceContext context, ReportActivity command, CancellationToken cancellationToken);

        public abstract Task CheckForExpirationAsync(InstanceContext instanceContext, CheckForExpiration command, CancellationToken cancellationToken);
    }
}