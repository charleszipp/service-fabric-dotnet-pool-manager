using PoolManager.Core.Builders;
using PoolManager.Core.Mediators;

namespace PoolManager.Core.Builders
{
    public abstract class MediatorDecorator : IMediatorBuilder
    {
        protected readonly IMediatorBuilder _builder;

        public MediatorDecorator(IMediatorBuilder builder)
        {
            _builder = builder;
        }

        public abstract Mediator Build();
    }
}
