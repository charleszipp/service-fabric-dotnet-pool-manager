using System.Collections.Generic;

namespace PoolManager.Domains.Instances.States
{
    public class InstanceStateProvider : IInstanceStateProvider
    {
        private readonly IDictionary<InstanceStates, InstanceState> _states = new Dictionary<InstanceStates, InstanceState>();

        public InstanceStateProvider(InstanceState idle, InstanceState vacant, InstanceState occupied)
        {
            _states[InstanceStates.Idle] = idle;
            _states[InstanceStates.Vacant] = vacant;
            _states[InstanceStates.Occupied] = occupied;
        }

        public InstanceState Get(InstanceStates state) => _states[state];
    }
}
