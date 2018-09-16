using PoolManager.Core.Commands;
using PoolManager.Core.Resolvers;
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
    }
}