using System;

namespace PoolManager.Domains.Instances.Interfaces
{
    public class CreateServiceResult
    {
        public CreateServiceResult(Uri ServiceUri)
        {
            this.ServiceUri = ServiceUri;
        }

        public Uri ServiceUri { get; }
    }
}