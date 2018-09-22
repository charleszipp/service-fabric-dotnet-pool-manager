using Ninject.Modules;
using PoolManager.Core.Mediators.Resolvers;

namespace PoolManager.Core.Mediators
{
    public class MediatorModule : NinjectModule
    {
        private readonly DependencyResolver resolver;

        public MediatorModule(DependencyResolver resolver = null)
        {
            this.resolver = resolver;
        }

        public override void Load()
        {
            Bind<DependencyResolver>().ToMethod(ctx => resolver ?? new NinjectDependencyResolver(ctx.Kernel));
            Bind<Mediator>().ToSelf();
        }
    }
}