using PoolManager.Core.Mediators;
using PoolManager.Core.Mediators.Builders;

namespace PoolManager.Domains.Pools
{
    public static class PoolsDomainFluentDecorator
    {
        public static IMediatorBuilder WithPools(this IMediatorBuilder builder) =>
            new PoolsDomainDecorator(builder);
    }

    public class PoolsDomainDecorator : MediatorDecorator
    {
        public PoolsDomainDecorator(IMediatorBuilder builder)
            : base(builder)
        {
        }

        public override Mediator Build()
        {
            return _builder
                .WithCommandHandler<StartPoolHandler, StartPool>()
                .WithCommandHandler<EnsurePoolSizeHandler, EnsurePoolSize>()
                .WithCommandHandler<PopVacantInstanceHandler, PopVacantInstance, PopVacantInstanceResult>()
                .WithCommandHandler<PushVacantInstanceHandler, PushVacantInstance>()
                .WithQueryHandler<GetPoolConfigurationHandler, GetPoolConfiguration, GetPoolConfigurationResult>()
                .Build();
        }
    }
}