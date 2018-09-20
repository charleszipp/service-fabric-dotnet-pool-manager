using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Commands
{
    public interface IHandleCommand<in TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }
}
