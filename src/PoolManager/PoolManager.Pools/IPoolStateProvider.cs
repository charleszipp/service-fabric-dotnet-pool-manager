using PoolManager.SDK.Pools;

namespace PoolManager.Pools
{
    public interface IPoolStateProvider
    {
        PoolState Get(PoolStates state);
    }
}