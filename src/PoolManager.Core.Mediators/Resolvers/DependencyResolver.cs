using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Queries;

namespace PoolManager.Core.Mediators.Resolvers
{
    public abstract class DependencyResolver
    {
        internal abstract T Single<T>();

        internal IHandleCommand<TCommand> GetCommandHandler<TCommand>() where TCommand : ICommand =>
            Single<IHandleCommand<TCommand>>();

        internal IHandleCommand<TCommand, TResult> GetCommandHandler<TCommand, TResult>() where TCommand : ICommand<TResult> =>
            Single<IHandleCommand<TCommand, TResult>>();

        internal IHandleQuery<TQuery, TResult> GetQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult> =>
            Single<IHandleQuery<TQuery, TResult>>();
    }
}