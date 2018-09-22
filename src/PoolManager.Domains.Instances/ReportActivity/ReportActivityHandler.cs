using PoolManager.Core.Mediators.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances
{
    public class ReportActivityHandler : IHandleCommand<ReportActivity, ReportActivityResult>
    {
        private readonly IInstanceRepository repository;

        public ReportActivityHandler(IInstanceRepository repository)
        {
            this.repository = repository;
        }

        public async Task<ReportActivityResult> ExecuteAsync(ReportActivity command, CancellationToken cancellationToken)
        {
            await repository.SetServiceLastActiveAsync(command.LastActiveUtc, cancellationToken);
            TimeSpan expirationQuanta = await repository.GetExpirationQuantaAsync(cancellationToken);
            return new ReportActivityResult(TimeSpan.FromMilliseconds(expirationQuanta.TotalMilliseconds / 3));
        }
    }
}
