using PoolManager.Core.Commands;
using PoolManager.Core.Mediators;

namespace PoolManager.Core.Builders
{
    public static class MediatorCommandFluentDecorator
    {
        public static IMediatorBuilder WithCommandHandler<TCommandHandler, TCommand>(this IMediatorBuilder builder)
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand => new MediatorCommandDecorator<TCommandHandler, TCommand>(builder);
    }

    public class MediatorCommandDecorator<TCommandHandler, TCommand> : MediatorDecorator
        where TCommandHandler : class, IHandleCommand<TCommand>
        where TCommand : ICommand
    {
        public MediatorCommandDecorator(IMediatorBuilder builder)
            : base(builder) { }

        public override Mediator Build()
        {
            var rvalue = _builder.Build();
            rvalue.DependencyResolver.RegisterCommandHandler<TCommandHandler, TCommand>();
            return rvalue;
        }
    }
}