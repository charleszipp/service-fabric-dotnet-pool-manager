using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Monitor.Interfaces
{
    public interface IMonitor
    {
        string Name { get; }

        Task<IEnumerable<ICommand>> RunAsync(CancellationToken cancellationToken);
    }
}