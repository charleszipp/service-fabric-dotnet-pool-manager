using Microsoft.ApplicationInsights.ServiceFabric.Remoting.Activities;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V1.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;

namespace PoolManager.Core
{
    public class PoolsActorService : ActorService
    {
        private readonly string endpointResourceName;

        public PoolsActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, string endpointResourceName, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) 
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            this.endpointResourceName = endpointResourceName;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var transportSettings = new FabricTransportRemotingListenerSettings
            {
                EndpointResourceName = endpointResourceName,
            };

            return new ServiceReplicaListener[1]
            {
                new ServiceReplicaListener(context => 
                    new FabricTransportActorServiceRemotingListener(context, 
                        new FabricTelemetryInitializingHandler(context,
                            new CorrelatingRemotingMessageHandler(this)
                        ), 
                        transportSettings
                    )
                )
            };
        }
    }
}