import { DimensionOperator } from 'types.bicep'

type AppGatewayMetric = {
  resourceType: 'Microsoft.Network/applicationGateways'
  metric:
    | 'ApplicationGatewayTotalTime'
    | 'FailedRequests'
    | 'UnhealthyHostCount'
    | 'ResponseStatus'
  dimensions: {
    name: 
      | 'BackendSettingsPool'
      | 'HttpStatusGroup'
    operator: DimensionOperator?
    values: string[]
  }[]?
}

type AppInsightsMetric = {
  resourceType: 'Microsoft.Insights/components'
  metric:
    | 'exceptions/count'
    | 'exceptions/server'
    | 'requests/failed'
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
    | 'ResiliencyConnectTimeouts'
    | 'ResiliencyRequestRetries'
    | 'ResiliencyRequestTimeouts'
    | 'ResponseTime'
    | 'RestartCount'
}

type EventGridCustomTopicMetric = {
  resourceType: 'Microsoft.EventGrid/topics'
  metric:
    | 'AdvancedFilterEvaluationCount'
    | 'DeadLetteredCount'
    | 'DeliveryAttemptFailCount'
    | 'DeliverySuccessCount'
    | 'DestinationProcessingDurationInMs'
    | 'DroppedEventCount'
    | 'MatchedEventCount'
    | 'PublishFailCount'
    | 'PublishSuccessCount'
    | 'PublishSuccessLatencyInMs'
    | 'UnmatchedEventCount'
}

type FileServiceMetric = {
  resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
  metric:
    | 'availability'
    | 'FileCapacity'
    | 'SuccessE2ELatency'
    dimensions: {
      name: 
        | 'FileShare'
        | 'Tier'
      operator: DimensionOperator?
      values: string[]
    }[]?
}

type PostgreSqlMetric = {
  resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
  dimensions: {
    name: 
      | 'DatabaseName'
    operator: DimensionOperator?
    values: string[]
  }[]?
  metric:
    | 'backup_storage_used'
    | 'client_connections_waiting'
    | 'connections_failed'
    | 'cpu_percent'
    | 'deadlocks'
    | 'disk_bandwidth_consumed_percentage'
    | 'disk_iops_consumed_percentage'
    | 'is_db_alive'
    | 'longest_query_time_sec'
    | 'longest_transaction_time_sec'
    | 'memory_percent'
    | 'storage_percent'
}

type SearchServiceMetric = {
  resourceType: 'Microsoft.Search/searchServices'
  metric:
    | 'DocumentsProcessedCount'
    | 'SearchLatency'
    | 'SearchQueriesPerSecond'
    | 'SkillExecutionCount'
    | 'ThrottledSearchQueriesPercentage'
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
  | AppGatewayMetric
  | AppInsightsMetric
  | AppServicePlanMetric
  | ContainerAppMetric
  | ContainerAppMetric
  | EventGridCustomTopicMetric
  | FileServiceMetric
  | PostgreSqlMetric
  | SearchServiceMetric
  | SiteMetric
  | StorageAccountMetric
