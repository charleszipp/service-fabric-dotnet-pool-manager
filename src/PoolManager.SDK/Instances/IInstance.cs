using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Instances.Requests;
using System;
using System.Threading.Tasks;

namespace PoolManager.SDK.Instances
{
    public interface IInstance : IActor
    {
        Task StartAsync(StartInstanceRequest request);

        Task StartAsAsync(StartInstanceAsRequest request);

        Task RemoveAsync();

        Task OccupyAsync(OccupyRequest request);

        Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request);

        Task VacateAsync();
    }
}