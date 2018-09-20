using PoolManager.Terminal.Commands;

namespace PoolManager.Terminal.Builders
{
    public static class PoolsCommandFluentDecorator
    {
        public static IPoolsBuilder WithCommandHandler<TCommandHandler, TCommand>(this IPoolsBuilder builder)
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand => new PoolsCommandDecorator<TCommandHandler, TCommand>(builder);
    }

    public class PoolsCommandDecorator<TCommandHandler, TCommand> : PoolsDecorator
        where TCommandHandler : class, IHandleCommand<TCommand>
        where TCommand : ICommand
    {
        public PoolsCommandDecorator(IPoolsBuilder builder)
            : base(builder) { }

        public override Pools Build()
        {
            var rvalue = _builder.Build();
            rvalue.DependencyResolver.RegisterCommandHandler<TCommandHandler, TCommand>();
            return rvalue;
        }
    }
}