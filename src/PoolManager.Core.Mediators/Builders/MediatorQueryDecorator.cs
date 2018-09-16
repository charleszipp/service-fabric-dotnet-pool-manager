using PoolManager.Core.Mediators.Queries;

namespace PoolManager.Core.Mediators.Builders
{
    public static class MediatorQueryFluentDecorator
    {
        public static IMediatorBuilder WithQueryHandler<TQueryHandler, TQuery, TResult>(this IMediatorBuilder builder)
            where TQueryHandler : class, IHandleQuery<TQuery, TResult>
            where TQuery : IQuery<TResult> => new MediatorQueryDecorator<TQueryHandler, TQuery, TResult>(builder);
    }

    public class MediatorQueryDecorator<TQueryHandler, TQuery, TResult> : MediatorDecorator
        where TQueryHandler : class, IHandleQuery<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        public MediatorQueryDecorator(IMediatorBuilder builder)
            : base(builder) { }

        public override Mediator Build()
        {
            var rvalue = _builder.Build();
            rvalue.DependencyResolver.RegisterQueryHandler<TQueryHandler, TQuery, TResult>();
            return rvalue;
        }
    }
}