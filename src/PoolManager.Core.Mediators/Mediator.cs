using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Invokers;
using PoolManager.Core.Mediators.Queries;
using PoolManager.Core.Mediators.Resolvers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Core.Mediators
{
    public class Mediator
    {
        public Mediator(DependencyResolver resolver)
        {
            DependencyResolver = resolver;
        }

        public DependencyResolver DependencyResolver { get; }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken)
        {
            var invoker = (VoidCommandInvoker)Activator.CreateInstance(typeof(VoidCommandInvoker<>).MakeGenericType(command.GetType()));
            return invoker.InvokeAsync(command, DependencyResolver, cancellationToken);
        }

        public Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        {
            var invoker = (CommandInvoker<TResult>)Activator.CreateInstance(typeof(CommandInvoker<,>).MakeGenericType(command.GetType(), typeof(TResult)));
            return invoker.InvokeAsync(command, DependencyResolver, cancellationToken);
        }

        public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var invoker = (QueryInvoker<TResult>)Activator.CreateInstance(typeof(QueryInvoker<,>).MakeGenericType(query.GetType(), typeof(TResult)));
            return invoker.InvokeAsync(query, DependencyResolver, cancellationToken);
        }
    }
}