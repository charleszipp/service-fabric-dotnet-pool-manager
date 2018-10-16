using PoolManager.Core.Mediators.Commands;

namespace PoolManager.Domains.Pools.Interfaces
{
    public class PopVacantInstance : ICommand<PopVacantInstanceResult>
    {
        public PopVacantInstance(string serviceTypeUri)
        {
            ServiceTypeUri = serviceTypeUri;
        }

        public string ServiceTypeUri { get; }
    }
}
