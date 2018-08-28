using PoolManager.SDK.Instances;

namespace PoolManager.Instances
{
    public interface IInstanceStateProvider
    {
        InstanceState Get(InstanceStates state);
    }
}
