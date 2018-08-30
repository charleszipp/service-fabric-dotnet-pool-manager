using PoolManager.SDK.Pools;
using System.Collections.Generic;

namespace PoolManager.Pools
{
    public class PoolStateProvider : IPoolStateProvider
    {
        private readonly IDictionary<PoolStates, PoolState> _states = new Dictionary<PoolStates, PoolState>();

        public PoolStateProvider(PoolState idle, PoolState active)
        {
            _states[PoolStates.Idle] = idle;
            _states[PoolStates.Active] = active;
        }

        public PoolState Get(PoolStates state) => _states[state];
    }
}