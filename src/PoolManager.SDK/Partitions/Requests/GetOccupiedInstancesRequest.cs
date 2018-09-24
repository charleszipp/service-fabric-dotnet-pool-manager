using System.Runtime.Serialization;

namespace PoolManager.SDK.Partitions.Requests
{
    [DataContract]
    public class GetOccupiedInstancesRequest
    {
        public GetOccupiedInstancesRequest(string serviceTypeUri)
        {
            ServiceTypeUri = serviceTypeUri;
        }

        [DataMember]
        public string ServiceTypeUri { get; private set; }
    }
}