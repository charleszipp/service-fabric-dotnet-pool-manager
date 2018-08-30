using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK.Pools.Responses
{
    [Serializable]
    [DataContract]
    public class ConfigurationResponse
    {
        [DataMember]
        public string ServiceTypeUri { get; private set; }
        public ConfigurationResponse(string serviceTypeUri)
        {
            ServiceTypeUri = serviceTypeUri;
        }
    }
}