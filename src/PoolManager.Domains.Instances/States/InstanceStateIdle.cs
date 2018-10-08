using System;
using System.Threading.Tasks;
using System.Threading;
using PoolManager.Domains.Instances.Interfaces;

namespace PoolManager.Domains.Instances.States
{
    public class InstanceStateIdle : InstanceState
    {
        public override InstanceStates State => InstanceStates.Idle;

        public override Task CheckForExpirationAsync(InstanceContext instanceContext, CheckForExpiration command, CancellationToken cancellationToken) => 
            throw new Exception("Cannot check for expiration against an idle instance");

        public override Task<InstanceState> OccupyAsync(InstanceContext context, OccupyInstance command, CancellationToken cancellationToken) => 
            throw new Exception("Invalid state transition. Cannot occupy an idle service.");

        public override Task<InstanceState> RemoveAsync(InstanceContext context, RemoveInstance command, CancellationToken cancellationToken) => 
            Task.FromResult<InstanceState>(this);

        public override Task<ReportActivityResult> ReportActivityAsync(InstanceContext context, ReportActivity command, CancellationToken cancellationToken)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to an idle instance");
            return Task.FromResult(new ReportActivityResult(TimeSpan.MaxValue));
        }

        public override async Task<InstanceState> StartAsync(InstanceContext context, StartInstance command, CancellationToken cancellationToken)
        {
            await context.Mediator.ExecuteAsync(command, cancellationToken);
            return context.InstanceStates.Get(InstanceStates.Vacant);
        }

        public override Task<InstanceState> VacateAsync(InstanceContext context, VacateInstance command, CancellationToken cancellationToken) => 
            throw new Exception("Invalid state transition. Cannot vacate an idle service.");
    }
}
