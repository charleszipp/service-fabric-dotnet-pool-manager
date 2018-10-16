using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances.Interfaces
{
    public class CheckForExpiration : ICommand
    {
        public CheckForExpiration(Guid instanceId, DateTime? asOfDate = null)
        {
            InstanceId = instanceId;
            AsOfDate = asOfDate ?? DateTime.UtcNow;
        }

        public Guid InstanceId { get; }

        public DateTime AsOfDate { get; }
    }
}