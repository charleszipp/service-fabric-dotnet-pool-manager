using PoolManager.Core.Mediators;
using PoolManager.Core.Mediators.Builders;

namespace PoolManager.Domains.Instances
{
    public static class InstancesDomainFluentDecorator
    {
        public static IMediatorBuilder WithInstances(this IMediatorBuilder builder) =>
            new InstancesDomainDecorator(builder);
    }

    public class InstancesDomainDecorator : MediatorDecorator
    {

        public InstancesDomainDecorator(IMediatorBuilder builder)
            : base(builder)
        {
        }

        public override Mediator Build()
        {
            return _builder
                .WithCommandHandler<StartInstanceHandler, StartInstance>()
                .WithCommandHandler<OccupyInstanceHandler, OccupyInstance>()                
                .WithCommandHandler<ReportActivityHandler, ReportActivity, ReportActivityResult>()
                .WithCommandHandler<VacateInstanceHandler, VacateInstance>()
                .WithCommandHandler<RemoveInstanceHandler, RemoveInstance>()
                .Build();
        }
    }
}