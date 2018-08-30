using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]

namespace PoolManager.Instances.Interfaces
{
    public interface IInstance : IActor
    {
        Task StartAsync(StartRequest request);

        Task StartAsAsync(StartAsRequest request);

        Task RemoveAsync();

        Task OccupyAsync(OccupyRequest request);

        Task ReportActivityAsync(ReportActivityRequest request);
    }
}