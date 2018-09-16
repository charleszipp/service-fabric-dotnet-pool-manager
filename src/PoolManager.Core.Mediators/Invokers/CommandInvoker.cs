using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Resolvers;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Core.Mediators.Invokers
{
    internal abstract class CommandInvoker<TResult>
    {
        internal abstract Task<TResult> InvokeAsync(ICommand<TResult> command, DependencyResolver resolver, CancellationToken cancellationToken);
    }

    internal class CommandInvoker<TCommand, TResult> : CommandInvoker<TResult>
        where TCommand : ICommand<TResult>
    {
        internal override Task<TResult> InvokeAsync(ICommand<TResult> command, DependencyResolver resolver, CancellationToken cancellationToken) =>
            resolver.GetCommandHandler<TCommand, TResult>().ExecuteAsync((TCommand)command, cancellationToken);
    }
}