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

        public NoOp(StatefulServiceContext context) : base(context)
        {
            _instanceProxy = new InstanceProxy(new CorrelatingActorProxyFactory(Context, callbackClient => new FabricTransportServiceRemotingClientFactory(callbackClient: callbackClient)));
        }

        public NoOp(StatefulServiceContext context, IReliableStateManagerReplica replica) : base(context, replica)
        {
        }

        public Task OccupyAsync(string instanceId, string serviceInstanceName)
        {
            _instanceId = Guid.Parse(instanceId);
            _serviceInstanceName = serviceInstanceName;
            Task.Run(() => ReportActivityAsync(DateTime.Now.AddMinutes(3)), _runCancellation);
            return Task.Delay(500);
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            _runCancellation = cancellationToken;
            return base.RunAsync(cancellationToken);
        }

        private async Task ReportActivityAsync(DateTime reportUntil)
        {
            var nextReportDateUtc = DateTime.UtcNow;

            while(DateTime.Now < reportUntil && !_runCancellation.IsCancellationRequested)
            {
                using (var op = _telemetryClient.StartOperation<RequestTelemetry>("NoOp.ReportActivityAsync"))
                {
                    var utcNow = DateTime.UtcNow;
                    if (utcNow >= nextReportDateUtc)
                    {
                        try
                        {
                            var reportActivityRequest = new ReportActivityRequest(_serviceInstanceName, utcNow);
                            var nextReportInterval = await _instanceProxy.ReportActivityAsync(_instanceId.Value, reportActivityRequest);
                            nextReportDateUtc = utcNow.Add(nextReportInterval);
                        }
                        catch (Exception ex)
                        {
                            op.Telemetry.Success = false;
                            _telemetryClient.TrackException(ex);
                        }
                    }
                }

                await Task.Delay(30000, _runCancellation);
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