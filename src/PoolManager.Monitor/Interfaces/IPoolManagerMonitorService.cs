using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using PoolManager.Monitor.Models;

namespace PoolManager.Monitor.Interfaces
{
    public interface IPoolManagerMonitorService : IService
    {
        /// <summary>
        /// Function to get orphans across all managed service types.
        /// </summary>
        /// <returns>A dictionary of orphans for each service type.</returns>
        Task<IDictionary<string, IEnumerable<OrphanInfo>>> GetOrphansAsync();
    }
}