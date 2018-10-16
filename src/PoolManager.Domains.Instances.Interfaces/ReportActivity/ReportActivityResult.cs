using System;

namespace PoolManager.Domains.Instances.Interfaces
{
    public class ReportActivityResult
    {
        public ReportActivityResult(TimeSpan nextReportTime)
        {
            NextReportTime = nextReportTime;
        }

        public TimeSpan NextReportTime { get; }
    }
}
