using PoolManager.Core.Mediators.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoolManager.Domains.Instances
{
    public class OccupyInstance : ICommand
    {
        public OccupyInstance(Guid instanceId, string instanceName)
        {
            InstanceId = instanceId;
            InstanceName = instanceName;
        }

        public Guid InstanceId { get; }
        public string InstanceName { get; }
    }
}
