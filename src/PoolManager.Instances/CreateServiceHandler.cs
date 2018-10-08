using PoolManager.Core;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Domains.Instances;
using PoolManager.Domains.Instances.Interfaces;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Instances
{
    public class CreateServiceHandler : IHandleCommand<CreateService, CreateServiceResult>
    {
        private readonly IClusterClient cluster;

        public CreateServiceHandler(IClusterClient cluster)
        {
            this.cluster = cluster;
        }

        public async Task<CreateServiceResult> ExecuteAsync(CreateService command, CancellationToken cancellationToken)
        {
            var partitionSchemeDescription = ToServiceFabricDescription(command.PartitionScheme);
            var serviceDescriptionFactory = new ServiceDescriptionFactory(command.ServiceTypeUri, command.InstanceId, partitionSchemeDescription);

            if (command.IsServiceStateful)
                await cluster.CreateStatefulServiceAsync(serviceDescriptionFactory, command.MinReplicas, command.TargetReplicas, command.HasPersistedState);
            else
                await cluster.CreateStatelessServiceAsync(serviceDescriptionFactory);

            return new CreateServiceResult(serviceDescriptionFactory.ServiceName);
        }

        private static System.Fabric.Description.PartitionSchemeDescription ToServiceFabricDescription(Domains.Instances.Interfaces.PartitionSchemeDescription desc)
        {
            switch (desc)
            {
                case Domains.Instances.Interfaces.PartitionSchemeDescription.UniformInt64Name:
                    return new UniformInt64RangePartitionSchemeDescription();

                case Domains.Instances.Interfaces.PartitionSchemeDescription.Named:
                    return new NamedPartitionSchemeDescription();

                default:
                    return new SingletonPartitionSchemeDescription();
            }
        }
    }
}
