﻿using System.Runtime.Serialization;

namespace PoolManager.SDK
{
    /// <summary>
    /// Enum describes all available service partitioning options.
    /// </summary>
    [DataContract]
    public enum PartitionSchemeDescription
    {
        [EnumMember]
        Singleton = 0,
        [EnumMember]
        UniformInt64Name = 1,
        [EnumMember]
        Named = 2
    };
}
