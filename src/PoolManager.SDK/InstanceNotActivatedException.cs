using System;
using System.Runtime.Serialization;

namespace PoolManager.SDK
{
    [Serializable]
    public class InstanceNotActivatedException : Exception
    {
        public InstanceNotActivatedException(string serviceTypeUri, string serviceInstanceName)
            : base("The instance has not completed activation, please try again later")
        {
            ServiceTypeUri = serviceTypeUri;
            ServiceInstanceName = serviceInstanceName;
        }

        protected InstanceNotActivatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ServiceTypeUri = info.GetString(nameof(ServiceTypeUri));
            ServiceInstanceName = info.GetString(nameof(ServiceInstanceName));
        }

        public string ServiceTypeUri { get; }

        public string ServiceInstanceName { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ServiceTypeUri), ServiceTypeUri);
            info.AddValue(nameof(ServiceInstanceName), ServiceInstanceName);
            base.GetObjectData(info, context);
        }
    }
}