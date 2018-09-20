using Ninject;
using System;

namespace PoolManager.Core.Mediators.Resolvers
{
    public class NinjectDependencyResolver : DependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver() : this(new StandardKernel())
        {
        }

        public NinjectDependencyResolver(IKernel kernel) => _kernel = kernel;

        internal override T Single<T>() =>
            _kernel.Get<T>();
    }
}