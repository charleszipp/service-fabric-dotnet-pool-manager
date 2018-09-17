namespace PoolManager.Domains.Instances.States
{
    public interface IInstanceStateProvider
    {
        InstanceState Get(InstanceStates state);
    }
}
