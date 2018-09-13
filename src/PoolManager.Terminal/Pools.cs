using PoolManager.Terminal.Commands;
using PoolManager.Terminal.Mediators;
using PoolManager.Terminal.Resolvers;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Terminal
{
    public class Pools
    {
        public DependencyResolver DependencyResolver { get; }
        internal Mediator Mediator { get; }

        public Pools(DependencyResolver resolver)
        {
            DependencyResolver = resolver;
            Mediator = new Mediator(resolver);
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken) => 
            Mediator.ExecuteAsync(command, cancellationToken);
    }    
}
