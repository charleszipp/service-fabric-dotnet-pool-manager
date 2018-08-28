using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ServiceFabric.Services.Remoting.V1;
using Microsoft.ServiceFabric.Services.Remoting.V1.Runtime;
using System.Fabric;
using System.Threading.Tasks;

namespace PoolManager.Core
{
    public class FabricTelemetryInitializingHandler : IServiceRemotingMessageHandler
    {
        private readonly ServiceContext _context;
        private readonly IServiceRemotingMessageHandler _handler;

        public FabricTelemetryInitializingHandler(ServiceContext context, IServiceRemotingMessageHandler handler)
        {
            _context = context;
            _handler = handler;
            FabricTelemetryInitializerExtension.SetServiceCallContext(_context);
        }

        public void HandleOneWay(IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            FabricTelemetryInitializerExtension.SetServiceCallContext(_context);
            _handler.HandleOneWay(requestContext, messageHeaders, requestBody);
        }

        public Task<byte[]> RequestResponseAsync(IServiceRemotingRequestContext requestContext, ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            FabricTelemetryInitializerExtension.SetServiceCallContext(_context);
            return _handler.RequestResponseAsync(requestContext, messageHeaders, requestBody);
        }
    }
}