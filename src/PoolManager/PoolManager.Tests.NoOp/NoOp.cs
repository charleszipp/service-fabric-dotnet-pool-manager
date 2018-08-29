using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PoolManager.SDK;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace PoolManager.Tests.NoOp
{
    internal sealed class NoOp : StatefulService, IServiceInstance
    {
        public NoOp(StatefulServiceContext context) : base(context)
        {
        }

        public NoOp(StatefulServiceContext context, IReliableStateManagerReplica replica) : base(context, replica)
        {
        }

        public Task OccupyAsync(string instanceId, string serviceInstanceName)
        {
            return Task.Delay(500);
        }

        public Task VacateAsync()
        {
            return Task.Delay(250);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(this.CreateServiceRemotingListener) };
        }
    }
}