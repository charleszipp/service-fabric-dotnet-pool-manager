using PoolManager.Domains.Instances.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances.States
{
    public class InstanceStateOccupied : InstanceState
    {
        public override InstanceStates State => InstanceStates.Occupied;

        public override Task CheckForExpirationAsync(InstanceContext instanceContext, CheckForExpiration command, CancellationToken cancellationToken) =>
            instanceContext.Mediator.ExecuteAsync(command, cancellationToken);

        public override Task<InstanceState> OccupyAsync(InstanceContext context, OccupyInstance command, CancellationToken cancellationToken) =>
            Task.FromResult<InstanceState>(this);

        public override async Task<InstanceState> RemoveAsync(InstanceContext context, RemoveInstance command, CancellationToken cancellationToken)
        {
            var rvalue = await VacateAsync(context, new VacateInstance(), cancellationToken);
            rvalue = await rvalue.RemoveAsync(context, new RemoveInstance(), cancellationToken);
            return rvalue;
        }

        public override Task<ReportActivityResult> ReportActivityAsync(InstanceContext context, ReportActivity command, CancellationToken cancellationToken) =>
            context.Mediator.ExecuteAsync(command, cancellationToken);

        public override Task<InstanceState> StartAsync(InstanceContext context, StartInstance command, CancellationToken cancellationToken) =>
            throw new Exception("Invalid state transition. Instance is already started");

        public override async Task<InstanceState> VacateAsync(InstanceContext context, VacateInstance command, CancellationToken cancellationToken)
        {
            await context.Mediator.ExecuteAsync(command, cancellationToken);
            return context.InstanceStates.Get(InstanceStates.Vacant);
        }
    }
}
