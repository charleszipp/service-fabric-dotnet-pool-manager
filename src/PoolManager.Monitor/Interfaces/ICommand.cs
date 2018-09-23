using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Monitor.Interfaces
{
    public interface ICommand
    {
        string Name { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}