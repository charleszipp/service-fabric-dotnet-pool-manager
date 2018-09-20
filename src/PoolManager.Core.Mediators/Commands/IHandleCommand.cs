using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Core.Mediators.Commands
{
    public interface IHandleCommand<in TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }

    public interface IHandleCommand<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }
}
