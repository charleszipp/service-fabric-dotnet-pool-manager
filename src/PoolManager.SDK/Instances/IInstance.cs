using Microsoft.ServiceFabric.Actors;
using PoolManager.SDK.Instances.Requests;
using PoolManager.SDK.Instances.Responses;
using System;
using System.Threading.Tasks;

namespace PoolManager.SDK.Instances
{
    public interface IInstance : IActor
    {
        Task StartAsync(StartInstanceRequest request);

        Task RemoveAsync();

        Task<OccupyResponse> OccupyAsync(OccupyRequest request);

        Task<TimeSpan> ReportActivityAsync(ReportActivityRequest request);

        Task VacateAsync();
    }
}