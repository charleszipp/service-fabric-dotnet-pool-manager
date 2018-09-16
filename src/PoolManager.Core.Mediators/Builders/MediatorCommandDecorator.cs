using PoolManager.Core.Mediators.Commands;

namespace PoolManager.Core.Mediators.Builders
{
    public static class MediatorCommandFluentDecorator
    {
        public static IMediatorBuilder WithCommandHandler<TCommandHandler, TCommand>(this IMediatorBuilder builder)
            where TCommandHandler : class, IHandleCommand<TCommand>
            where TCommand : ICommand => new MediatorCommandDecorator<TCommandHandler, TCommand>(builder);

        public static IMediatorBuilder WithCommandHandler<TCommandHandler, TCommand, TResult>(this IMediatorBuilder builder)
            where TCommandHandler : class, IHandleCommand<TCommand, TResult>
            where TCommand : ICommand<TResult> => new MediatorCommandDecorator<TCommandHandler, TCommand, TResult>(builder);
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

    public class MediatorCommandDecorator<TCommandHandler, TCommand, TResult> : MediatorDecorator
        where TCommandHandler : class, IHandleCommand<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        public MediatorCommandDecorator(IMediatorBuilder builder)
            : base(builder) { }

        public override Mediator Build()
        {
            var rvalue = _builder.Build();
            rvalue.DependencyResolver.RegisterCommandHandler<TCommandHandler, TCommand, TResult>();
            return rvalue;
        }
    }
}