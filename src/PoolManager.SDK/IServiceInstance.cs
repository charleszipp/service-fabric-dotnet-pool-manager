using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolManager.SDK
{
    public interface IServiceInstance : IService
    {
        /// <summary>
        /// This is called when an instance is activated.
        /// </summary>
        /// <param name="instanceActorId"></param>
        /// <param name="serviceInstanceName"></param>
        /// <returns></returns>
        Task OccupyAsync(string instanceId, string serviceInstanceName);

        /// <summary>
        /// This is called when an instance is deactivated and returned back to the pool.
        /// </summary>
        /// <returns></returns>
        Task VacateAsync();
    }
}
