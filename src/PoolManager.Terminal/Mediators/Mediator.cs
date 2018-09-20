using PoolManager.Terminal.Commands;
using PoolManager.Terminal.Resolvers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal.Mediators
{
    public class Mediator
    {
        private readonly DependencyResolver _resolver;

        public Mediator(DependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken)
        {
            var invoker = (VoidCommandInvoker)Activator.CreateInstance(typeof(VoidCommandInvoker<>).MakeGenericType(command.GetType()));
            return invoker.InvokeAsync(command, _resolver, cancellationToken);
        }
    }
}