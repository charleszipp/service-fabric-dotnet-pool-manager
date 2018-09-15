using System;
using System.Runtime.Serialization;

namespace PoolManager.Partitions.Models
{
    [DataContract]
    public class MappedInstance
    {
        public MappedInstance(Guid id, Uri serviceName)
        {
            Id = id;
            ServiceName = serviceName;
        }

        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public Uri ServiceName { get; private set; }
    }
}