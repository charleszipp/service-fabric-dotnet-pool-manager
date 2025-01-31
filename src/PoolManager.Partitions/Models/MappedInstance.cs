﻿using System;
using System.Runtime.Serialization;

namespace PoolManager.Partitions.Models
{
    [DataContract]
    public class MappedInstance
    {
        public MappedInstance(Guid id, Uri serviceName, string instanceName)
        {
            Id = id;
            ServiceName = serviceName;
            InstanceName = instanceName;
        }

        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public Uri ServiceName { get; private set; }
        [DataMember]
        public string InstanceName { get; private set; }
    }
}