using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Resolvers;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Core.Mediators.Invokers
{
    internal abstract class VoidCommandInvoker
    {
        internal abstract Task InvokeAsync(ICommand command, DependencyResolver resolver, CancellationToken cancellationToken);
    }

    internal class VoidCommandInvoker<TCommand> : VoidCommandInvoker
        where TCommand : ICommand
    {
        internal override Task InvokeAsync(ICommand command, DependencyResolver resolver, CancellationToken cancellationToken) =>
            resolver.GetCommandHandler<TCommand>().ExecuteAsync((TCommand)command, cancellationToken);
    }
}