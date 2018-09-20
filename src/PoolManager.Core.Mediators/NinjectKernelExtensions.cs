using Ninject;
using PoolManager.Core.Mediators.Commands;
using PoolManager.Core.Mediators.Queries;
using PoolManager.Core.Mediators.Resolvers;

namespace PoolManager.Core.Mediators
{
    public static class NinjectKernelExtensions
    {
        public static IKernel WithMediator(this IKernel kernel, DependencyResolver resolver = null)
        {
            kernel.Load(new MediatorModule(resolver));
            return kernel;
        }
        public static IKernel WithCommandHandler<TCommandHandler, TCommand>(this IKernel kernel)
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand
        {
            kernel.Bind<IHandleCommand<TCommand>>().To<TCommandHandler>();
            return kernel;
        }
        public static IKernel WithCommandHandler<TCommandHandler, TCommand, TResult>(this IKernel kernel)
            where TCommandHandler : class, IHandleCommand<TCommand, TResult>
            where TCommand : ICommand<TResult>
        {
            kernel.Bind<IHandleCommand<TCommand, TResult>>().To<TCommandHandler>();
            return kernel;
        }
        public static IKernel WithQueryHandler<TQueryHandler, TQuery, TResult>(this IKernel kernel)
            where TQueryHandler : class, IHandleQuery<TQuery, TResult>
            where TQuery : IQuery<TResult>
        {
            kernel.Bind<IHandleQuery<TQuery, TResult>>().To<TQueryHandler>();
            return kernel;
        }
    }
}