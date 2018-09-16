using PoolManager.SDK.Pools;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolStateIdle : PoolState
    {
        public override PoolStates State => PoolStates.Idle;
    }
}