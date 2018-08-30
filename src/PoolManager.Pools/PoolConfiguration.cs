using PoolManager.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PoolManager.Pools
{
    /// <summary>
    /// This is the class holding the properties for a pool configuration in the 
    /// Pool Manager.
    /// </summary>
    [DataContract]
    public class PoolConfiguration
    {
        /// <summary>
        /// Is the service stateful or stateless?
        /// </summary>
        [DataMember]
        public bool IsServiceStateful { get; set; } = true;

        /// <summary>
        /// The service type this configuration is for (including appname)
        /// </summary>
        [DataMember]
        public string ServiceTypeUri { get; set; }
        /// <summary>
        /// Minimum amount of replica for the stateful service
        /// </summary>
        [DataMember]
        public int MinReplicaSetSize { get; set; } = 1;
        /// <summary>
        /// Target number of replicas for the stateful service
        /// </summary>
        [DataMember]
        public int TargetReplicasetSize { get; set; } = 3;
        /// <summary>
        /// Does this service have persisted state?
        /// </summary>
        [DataMember]
        public bool HasPersistedState { get; set; } = true;
        /// <summary>
        /// Select partitioning for pooled services
        /// </summary>
        [DataMember]
        public PartitionSchemeDescription PartitionScheme { get; set; } = PartitionSchemeDescription.UniformInt64Name;

        /// <summary>
        /// Maximum size of the service instance pool for a service type. 
        /// Number of instances in the pool will never be more than this.
        /// If maximum reached, pool manager doesn't allocate new idle instance, 
        /// however GetServiceAsync() allocates new instance on demand 
        /// regardless of maximum pool size.
        /// </summary>
        [DataMember]
        public int MaxPoolSize { get; set; } = Int32.MaxValue;
        /// <summary>
        /// Num of items the service instance pool will grow/shrink with
        /// when reallocated.
        /// </summary>
        [DataMember]
        public int IdleServicesPoolSize { get; set; } = 10;
        /// <summary>
        /// Amount of services that can be allocated in parallel.
        /// </summary>
        [DataMember]
        public int ServicesAllocationBlockSize { get; set; } = 5;

        /// <summary>
        /// Set the time quanta that when expired force the service instance to be
        /// deactivated and returned to the idle instance pool. Use ReportActivityAsync
        /// in the IServiceManagerCallback interface to report activity and reset
        /// the timer that checks for this quanta.
        /// </summary>
        [DataMember]
        public TimeSpan ExpirationQuanta { get; set; } = new TimeSpan(24, 0, 0);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"IsServiceStateful={IsServiceStateful}");
            sb.Append($"MinReplicaSetSize={MinReplicaSetSize}; ");
            sb.Append($"TargetReplicasetSize={TargetReplicasetSize}; ");
            sb.Append($"HasPersistedState={HasPersistedState}; ");
            sb.Append($"PartitionScheme={PartitionScheme}");
            sb.Append($"IdleServicesPoolSize={IdleServicesPoolSize}; ");
            sb.Append($"MaxPoolSize={MaxPoolSize}; ");
            sb.Append($"ServicesAllocationBlockSize={ServicesAllocationBlockSize}; ");
            sb.Append($"ExpirationQuanta={ExpirationQuanta}");
            return sb.ToString();
        }
    }
}
