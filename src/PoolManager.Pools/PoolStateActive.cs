using PoolManager.SDK.Pools;
using PoolManager.SDK.Pools.Requests;
using PoolManager.SDK.Pools.Responses;
using System.Linq;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    public class PoolStateActive : PoolState
    {
        public override PoolStates State => PoolStates.Active;
    }
}