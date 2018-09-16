using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Core.Mediators.Queries
{
    public interface IHandleQuery<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
    }
}