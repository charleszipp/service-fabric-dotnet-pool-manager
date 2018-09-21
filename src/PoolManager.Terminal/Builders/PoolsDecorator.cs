namespace PoolManager.Terminal.Builders
{
    public abstract class PoolsDecorator : IPoolsBuilder
    {
        protected readonly IPoolsBuilder _builder;

        public PoolsDecorator(IPoolsBuilder builder)
        {
            _builder = builder;
        }

        public abstract Pools Build();
    }
}
