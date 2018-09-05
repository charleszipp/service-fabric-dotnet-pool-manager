Feature: PoolsFeature


Scenario: pool start creates service instances with provided configuration
	Given the idle pool "fabric:/PoolManager.Tests/NoOp"
	When the pool "fabric:/PoolManager.Tests/NoOp" is started with the following configuration
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
	Then the "fabric:/PoolManager.Tests/NoOp" pool configuration should be	
	| Field                       | Value                          |
	| ExpirationQuanta            | 00:03:00                       |
	| HasPersistedState           | true                           |
	| IdleServicesPoolSize        | 3                              |
	| IsServiceStateful           | true                           |
	| MaxPoolSize                 | 10                             |
	| MinReplicaSetSize           | 1                              |
	| PartitionScheme             | Singleton                      |
	| ServicesAllocationBlockSize | 2                              |
	| ServiceTypeUri              | fabric:/PoolManager.Tests/NoOp |
	| TargetReplicasetSize        | 3                              |
	And there should be "3" service instances for service fabric application "fabric:/PoolManager.Tests" and service type "NoOp"
