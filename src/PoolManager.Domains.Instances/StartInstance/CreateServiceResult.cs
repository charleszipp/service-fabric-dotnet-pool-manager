using System;

namespace PoolManager.Domains.Instances
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