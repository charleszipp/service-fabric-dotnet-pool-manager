using PoolManager.Terminal.Resolvers;

namespace PoolManager.Terminal.Builders
{
    public class PoolsBuilder : IPoolsBuilder
    {
        private readonly DependencyResolver _resolver;

        public PoolsBuilder(DependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public Pools Build() => new Pools(_resolver);
    }
}
