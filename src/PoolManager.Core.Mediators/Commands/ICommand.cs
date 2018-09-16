namespace PoolManager.Core.Mediators.Commands
{
    public interface ICommand
    {
    }

    public interface ICommand<TResult> : ICommand
    {
    }
}
