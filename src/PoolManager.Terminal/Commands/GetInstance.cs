using CommandLine;
using PoolManager.Core.Mediators.Commands;
using PoolManager.SDK.Partitions;
using PoolManager.SDK.Partitions.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    [Verb("get-instance")]
    public class GetInstance : ICommand
    {
        public GetInstance(
            string partitionId,
            string name,
            string serviceTypeUri = Constants.NoOpServiceTypeUri
            )
        {
            PartitionId = partitionId;
            ServiceTypeUri = serviceTypeUri;
            Name = name;
        }

        [Option('p', "partition")]
        public string PartitionId { get; }
        [Option('n', "name")]
        public string Name { get; }
        [Option('u', "uri", Default = Constants.NoOpServiceTypeUri)]
        public string ServiceTypeUri { get; }
    }

    public class GetInstanceHandler : IHandleCommand<GetInstance>
    {
        private readonly IPartitionProxy _partitions;
        public GetInstanceHandler(IPartitionProxy partitions) => 
            _partitions = partitions;
        public Task ExecuteAsync(GetInstance command, CancellationToken cancellationToken) => 
            _partitions.GetInstanceAsync(command.PartitionId, new GetInstanceRequest(command.ServiceTypeUri, command.Name));
    }
}
