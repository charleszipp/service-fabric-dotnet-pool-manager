Feature: PoolManagerFeature

@ignore
Scenario: pool start creates service instances with provided configuration
	Given the service fabric application name "fabric:/PoolManager.Tests" for type "PoolManager.Tests" exists with no services
	And the service fabric application type "PoolManager.Tests" has "NoOp" service type
	And the service pool "fabric:/PoolManager.Tests/NoOp" does not exist
	When the "fabric:/PoolManager.Tests/NoOp" pool is started with the following configuration
	| Field                       | Value     |
	| IsServiceStateful           | true      |
	| HasPersistedState           | true      |
	| MinReplicas                 | 1         |
	| TargetReplicas              | 3         |
	| PartitionScheme             | Singleton |
	| MaxPoolSize                 | 10        |
	| IdleServicesPoolSize        | 3         |
	| ServicesAllocationBlockSize | 2         |
	| ExpirationQuanta            | 00:03:00  |
	Then the pool configuration should be
	| Field                       | Value     |
	| IsServiceStateful           | true      |
	| HasPersistedState           | true      |
	| MinReplicas                 | 1         |
	| TargetReplicas              | 3         |
	| PartitionScheme             | Singleton |
	| MaxPoolSize                 | 10        |
	| IdleServicesPoolSize        | 3         |
	| ServicesAllocationBlockSize | 2         |
	| ExpirationQuanta            | 00:03:00  |
	And there should be "10" service instances for service fabric application "fabric:/PoolManager.Tests" and service type "NoOp"
	And each service fabric application "fabric:/PoolManager.Tests" and service type "NoOp" instance should have the following configuration
	| Field             | Value     |
	| MinReplicaSize    | 1         |
	| TargetReplicaSize | 3         |
	| PartitionScheme   | Singleton |
	| IsServiceStateful | true      |
	| HasPersistedState | true      |
	And each service fabric application "fabric:/PoolManager.Tests" and service type "NoOp" instance partition should be healthy