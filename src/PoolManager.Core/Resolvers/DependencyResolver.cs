using PoolManager.Core.Commands;

namespace PoolManager.Core.Resolvers
{
    public abstract class DependencyResolver
    {
        protected abstract void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface;

        protected abstract T Single<T>();

        internal IHandleCommand<TCommand> GetCommandHandler<TCommand>() where TCommand : ICommand =>
            Single<IHandleCommand<TCommand>>();

        internal void RegisterCommandHandler<TCommandHandler, TCommand>()
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand =>
            RegisterTransient<IHandleCommand<TCommand>, TCommandHandler>();
    }
}
