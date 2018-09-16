using System;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Queries;

namespace PoolManager.Core.Mediators.Resolvers
{
    public abstract class DependencyResolver
    {
        protected abstract void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface;

        protected abstract T Single<T>();

        internal IHandleCommand<TCommand> GetCommandHandler<TCommand>() where TCommand : ICommand =>
            Single<IHandleCommand<TCommand>>();

        internal IHandleCommand<TCommand, TResult> GetCommandHandler<TCommand, TResult>() where TCommand : ICommand<TResult> =>
            Single<IHandleCommand<TCommand, TResult>>();

        internal IHandleQuery<TQuery, TResult> GetQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult> =>
            Single<IHandleQuery<TQuery, TResult>>();

        internal void RegisterCommandHandler<TCommandHandler, TCommand>()
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand =>
            RegisterTransient<IHandleCommand<TCommand>, TCommandHandler>();

        internal void RegisterQueryHandler<TQueryHandler, TQuery, TResult>()
            where TQueryHandler : class, IHandleQuery<TQuery, TResult>
            where TQuery : IQuery<TResult> =>
            RegisterTransient<IHandleQuery<TQuery, TResult>, TQueryHandler>();

        internal void RegisterCommandHandler<TCommandHandler, TCommand, TResult>()
            where TCommandHandler : class, IHandleCommand<TCommand, TResult>
            where TCommand : ICommand<TResult> =>
            RegisterTransient<IHandleCommand<TCommand, TResult>, TCommandHandler>();
    }
}
