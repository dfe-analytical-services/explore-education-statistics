type AppGatewayMetric = {
  resourceType: 'Microsoft.Network/applicationGateways'
  metric: 
    | 'ApplicationGatewayTotalTime'
}

type AppServicePlanMetric = {
  resourceType: 'Microsoft.Web/serverfarms'
  metric: 
    | 'CpuPercentage'
    | 'MemoryPercentage'
}

type ContainerAppMetric = {
  resourceType: 'Microsoft.App/containerApps'
  metric: 
    | 'CpuPercentage' 
    | 'MemoryPercentage'
    | 'ResponseTime'
    | 'RestartCount'
}

type FileServiceMetric = {
  resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
  metric: 
    | 'SuccessE2ELatency'
}

type PostgreSqlMetric = {
  resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
  metric: 
    | 'cpu_percent'
    | 'memory_percent'
}

type StorageAccountMetric = {
  resourceType: 'Microsoft.Storage/storageAccounts'
  metric: 
    | 'SuccessE2ELatency'
}



@export()
@discriminator('resourceType')
type ResourceMetric = 
| AppServicePlanMetric
| AppGatewayMetric
| ContainerAppMetric
| ContainerAppMetric
| FileServiceMetric
| PostgreSqlMetric
| StorageAccountMetric
