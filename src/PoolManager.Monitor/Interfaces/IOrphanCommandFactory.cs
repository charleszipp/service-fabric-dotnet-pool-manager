using PoolManager.Monitor.Models;
using PoolManager.Monitor.Orphans;

namespace PoolManager.Monitor.Interfaces
{
    public interface IRemoveOrphanCommandFactory
    {
        RemoveOrphanCommand CreateRemoveOrphanCommand(OrphanInfo orphan);
    }
}