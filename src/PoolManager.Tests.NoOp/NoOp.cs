using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PoolManager.Core;
using PoolManager.SDK;
using PoolManager.SDK.Instances;
using PoolManager.SDK.Instances.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Tests.NoOp
{
    internal sealed class NoOp : StatefulService, IServiceInstance
    {
        private readonly IInstanceProxy _instanceProxy;
        private Guid? _instanceId;
        private string _serviceInstanceName;
        private CancellationToken _runCancellation = default(CancellationToken);
        private readonly TelemetryClient _telemetryClient = new TelemetryClient();
        private DateTime? _nextReportDateUtc = null;

        public NoOp(StatefulServiceContext context) : base(context)
        {
            _instanceProxy =
                new InstanceProxy(
                    new CorrelatingActorProxyFactory(Context,
                        callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)),
                    new GuidGetter());
        }
        public NoOp(StatefulServiceContext context, IReliableStateManagerReplica replica) : base(context, replica)
        {
        }
        public async Task OccupyAsync(string instanceId, string serviceInstanceName)
        {
            _instanceId = Guid.Parse(instanceId);
            _serviceInstanceName = serviceInstanceName;
            _nextReportDateUtc = DateTime.UtcNow;
            await Task.Delay(500);
        }
        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            _runCancellation = cancellationToken;
            return base.RunAsync(cancellationToken);
        }
        private async Task ReportActivityAsync()
        {
            var utcNow = DateTime.UtcNow;
            if (_nextReportDateUtc.HasValue && utcNow >= _nextReportDateUtc.Value)
            {
                var reportActivityRequest = new ReportActivityRequest(utcNow);
                var nextReportInterval = await _instanceProxy.ReportActivityAsync(_instanceId.Value, reportActivityRequest);
                _nextReportDateUtc = utcNow.Add(nextReportInterval);
            }
        }
        public Task VacateAsync()
        {
            _instanceId = null;
            _serviceInstanceName = null;
            return Task.Delay(250);
        }
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[1]
            {
                new ServiceReplicaListener(context =>
                    new FabricTransportServiceRemotingListener(context,
                            new FabricTelemetryInitializingHandler(context,
                                new CorrelatingRemotingMessageHandler(context, this)
                            )
                        )
                    )
            };
        }
    }
}