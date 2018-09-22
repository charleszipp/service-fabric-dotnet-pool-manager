using PoolManager.Core.Mediators.Commands;
using System;

namespace PoolManager.Domains.Instances
{
    public class DeleteService : ICommand
    {
        public DeleteService(Uri serviceName)
        {
            ServiceName = serviceName;
        }

        public Uri ServiceName { get; }
    }
}
