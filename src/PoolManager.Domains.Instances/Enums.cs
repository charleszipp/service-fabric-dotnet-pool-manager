namespace PoolManager.Domains.Instances
{
    public enum PartitionSchemeDescription
    {
        Singleton = 0,
        UniformInt64Name = 1,
        Named = 2
    }

    public enum InstanceStates
    {
        Idle = 0,
        Vacant = 1,
        Occupied = 2
    }
}
