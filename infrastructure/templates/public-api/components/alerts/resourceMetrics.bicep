import { DimensionOperator } from 'types.bicep'

type AppGatewayMetric = {
  resourceType: 'Microsoft.Network/applicationGateways'
  metric:
    | 'ApplicationGatewayTotalTime'
    | 'FailedRequests'
    | 'UnhealthyHostCount'
  dimensions: {
    name: 'BackendSettingsPool'
    operator: DimensionOperator?
    values: string[]
  }[]?
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
    | 'availability'
    | 'FileCapacity'
    | 'SuccessE2ELatency'
    dimensions: {
      name: 'FileShare' | 'Tier'
      operator: DimensionOperator?
      values: string[]
    }[]?
}

type PostgreSqlMetric = {
  resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
  metric:
    | 'backup_storage_used'
    | 'client_connections_waiting'
    | 'cpu_percent'
    | 'disk_bandwidth_consumed_percentage'
    | 'disk_iops_consumed_percentage'
    | 'is_db_alive'
    | 'longest_query_time_sec'
    | 'longest_transaction_time_sec'
    | 'memory_percent'
    | 'storage_percent'
}

type SiteMetric = {
  resourceType: 'Microsoft.Web/sites'
  metric:
    | 'HealthCheckStatus'
    | 'Http401'
    | 'Http403'
    | 'Http4xx'
    | 'Http5xx'
}

type StorageAccountMetric = {
  resourceType: 'Microsoft.Storage/storageAccounts'
  metric:
    | 'availability'
    | 'SuccessE2ELatency'
    | 'UsedCapacity'
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
| SiteMetric
| StorageAccountMetric
