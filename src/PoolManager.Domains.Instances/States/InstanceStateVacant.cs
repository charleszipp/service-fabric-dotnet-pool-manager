using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances.States
{
    public class InstanceStateVacant : InstanceState
    {
        public override InstanceStates State => InstanceStates.Vacant;

        public override async Task<InstanceState> OccupyAsync(InstanceContext context, OccupyInstance command, CancellationToken cancellationToken)
        {
            await context.Mediator.ExecuteAsync(command, cancellationToken);
            return context.InstanceStates.Get(InstanceStates.Occupied);
        }

        public override Task<ReportActivityResult> ReportActivityAsync(InstanceContext context, ReportActivity command, CancellationToken cancellationToken)
        {
            context.TelemetryClient.TrackTrace("Activity was reported to a vacant instance");
            return Task.FromResult(new ReportActivityResult(TimeSpan.MaxValue));
        }

        public override async Task<InstanceState> RemoveAsync(InstanceContext context, RemoveInstance command, CancellationToken cancellationToken)
        {
            await context.Mediator.ExecuteAsync(command, cancellationToken);
            return context.InstanceStates.Get(InstanceStates.Idle);
        }

        public override Task<InstanceState> StartAsync(InstanceContext context, StartInstance command, CancellationToken cancellationToken) =>
            throw new Exception($"Invalid state transition. Instance is already started");

        public override Task<InstanceState> VacateAsync(InstanceContext context, VacateInstance command, CancellationToken cancellationToken) =>
            Task.FromResult<InstanceState>(this);

        public override Task CheckForExpirationAsync(InstanceContext instanceContext, CheckForExpiration command, CancellationToken cancellationToken) =>
            throw new Exception("Cannot check for expiration against a vacant instance");
    }
}