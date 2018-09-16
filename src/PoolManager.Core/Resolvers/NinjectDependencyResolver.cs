using Ninject;

namespace PoolManager.Core.Resolvers
{
    public class NinjectDependencyResolver : DependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver() : this(new StandardKernel()) { }

        public NinjectDependencyResolver(IKernel kernel) => _kernel = kernel;

        protected override void RegisterTransient<TInterface, TImplementation>() =>
            _kernel.Bind<TInterface>().To<TImplementation>().InTransientScope();

        protected override T Single<T>() =>
            _kernel.Get<T>();
    }
}
