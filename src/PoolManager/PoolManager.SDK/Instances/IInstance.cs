using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Instances.Requests;
using System.Threading.Tasks;

namespace PoolManager.SDK.Instances
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