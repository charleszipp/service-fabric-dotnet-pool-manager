using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances.Interfaces
{
    public class ReportActivity : ICommand<ReportActivityResult>
    {
        public ReportActivity(DateTime lastActiveUtc)
        {
            LastActiveUtc = lastActiveUtc;
        }

        public DateTime LastActiveUtc { get; }
    }
}
