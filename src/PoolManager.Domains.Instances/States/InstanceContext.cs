using Microsoft.ApplicationInsights;
using PoolManager.Core.Mediators;
using System.Threading;
using System.Threading.Tasks;

namespace PoolManager.Domains.Instances.States
{
    public class InstanceContext
    {
        private readonly IInstanceRepository repository;
        private InstanceState _currentState;

        internal TelemetryClient TelemetryClient { get; }
        internal Mediator Mediator { get; }
        internal IInstanceStateProvider InstanceStates { get; }

        public InstanceContext(TelemetryClient telemetryClient, Mediator mediator, IInstanceRepository repository, IInstanceStateProvider states)
        {
            TelemetryClient = telemetryClient;
            Mediator = mediator;
            InstanceStates = states;
            this.repository = repository;
            _currentState = InstanceStates.Get(Instances.InstanceStates.Idle);
        }

        public async Task ActivateAsync(CancellationToken cancellationToken)
        {
            var instanceState = await repository.TryGetInstanceStateAsync(cancellationToken);
            if (instanceState.HasValue)
                _currentState = InstanceStates.Get(instanceState.Value);
        }

        public Task DeactivateAsync(CancellationToken cancellationToken) =>
            repository.SetInstanceStateAsync(_currentState.State, cancellationToken);

        public async Task StartAsync(StartInstance command, CancellationToken cancellationToken)
        {
            _currentState = await _currentState.StartAsync(this, command, cancellationToken);
            await repository.SetInstanceStateAsync(_currentState.State, cancellationToken);
        }

        public async Task OccupyAsync(OccupyInstance command, CancellationToken cancellationToken)
        {
            _currentState = await _currentState.OccupyAsync(this, command, cancellationToken);
            await repository.SetInstanceStateAsync(_currentState.State, cancellationToken);
        }

        public async Task VacateAsync(VacateInstance command, CancellationToken cancellationToken)
        {
            _currentState = await _currentState.VacateAsync(this, command, cancellationToken);
            await repository.SetInstanceStateAsync(_currentState.State, cancellationToken);
        }

        public async Task RemoveAsync(RemoveInstance command, CancellationToken cancellationToken)
        {
            _currentState = await _currentState.RemoveAsync(this, command, cancellationToken);
            await repository.SetInstanceStateAsync(_currentState.State, cancellationToken);
        }

        public Task<ReportActivityResult> ReportActivityAsync(ReportActivity command, CancellationToken cancellationToken) =>
            _currentState.ReportActivityAsync(this, command, cancellationToken);

        public Task CheckForExpirationAsync(CheckForExpiration command, CancellationToken cancellationToken) =>
            _currentState.CheckForExpirationAsync(this, command, cancellationToken);
    }
}