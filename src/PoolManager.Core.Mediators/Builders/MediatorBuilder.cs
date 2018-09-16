using PoolManager.Core.Mediators.Resolvers;

namespace PoolManager.Core.Mediators.Builders
{
    public class MediatorBuilder : IMediatorBuilder
    {
        private readonly DependencyResolver _resolver;

        public MediatorBuilder(DependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public Mediator Build() => new Mediator(_resolver);
    }
}
