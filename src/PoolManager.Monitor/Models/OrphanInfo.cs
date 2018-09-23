using System.Fabric.Health;
using System.Fabric.Query;
using System.Runtime.Serialization;

namespace PoolManager.Monitor.Models
{
    /// <summary>
    /// This structure holds service information for an orphaned service instance
    /// </summary>
    [DataContract]
    public class OrphanInfo
    {
        /// <summary>
        /// Name of the service instance
        /// </summary>
        [DataMember]
        public string ServiceName { get; private set; }

        /// <summary>
        /// The service type uri this orphan is for
        /// </summary>
        public string ServiceTypeUri { get; private set; }

        /// <summary>
        /// Service current health state
        /// </summary>
        [DataMember]
        public HealthState HealthState { get; private set; }

        /// <summary>
        /// Service current status
        /// </summary>
        [DataMember]
        public ServiceStatus ServiceStatus { get; private set; }

        public OrphanInfo(string serviceName, string serviceTypeUri, HealthState healthState, ServiceStatus serviceStatus)
        {
            ServiceName = serviceName;
            ServiceTypeUri = serviceTypeUri;
            HealthState = healthState;
            ServiceStatus = serviceStatus;
        }
    }
}