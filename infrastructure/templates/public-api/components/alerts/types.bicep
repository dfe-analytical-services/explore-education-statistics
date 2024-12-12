@export()
type EvaluationFrequency = 
  | 'PT1M'
  | 'PT5M'
  | 'PT15M'
  | 'PT30M'
  | 'PT1H'

@export()
type WindowSize = 
  | 'PT5M'
  | 'PT15M'
  | 'PT30M'
  | 'PT1H'

@export()
type DynamicMetricOperator =
  | 'GreaterOrLessThan' 
  | 'GreaterThan'
  | 'LessThan'

@export()
type StaticMetricOperator =
  | 'Equals'
  | 'GreaterThan'
  | 'GreaterThanOrEqual'
  | 'LessThan'
  | 'LessThanOrEqual'  

@export()
type TimeAggregation = 
  | 'Average'
  | 'Count'
  | 'Maximum'
  | 'Minimum'
  | 'Total'

@export()
type Severity = 
  | 'Critical'
  | 'Error'
  | 'Warning'
  | 'Informational'
  | 'Verbose'

@export()
type Sensitivity = 
  | 'High'
  | 'Medium'
  | 'Low'

@export()
var severityMapping = {
  Critical: 0
  Error: 1
  Warning: 2
  Informational: 3
  Verbose: 4
}

@export()
type ResourceType = 
  | 'Microsoft.App/containerApps'
  | 'Microsoft.DBforPostgreSQL/flexibleServers'
  | 'Microsoft.Network/applicationGateways'
  | 'Microsoft.Sql/servers/databases'
  | 'Microsoft.Storage/storageAccounts'
  | 'Microsoft.Storage/storageAccounts/fileServices'
  | 'Microsoft.Web/serverfarms'
  | 'Microsoft.Web/sites' 
  | 'Microsoft.Web/sites/slots'

@export()
type MetricName = 
  | 'ApplicationGatewayTotalTime'
  | 'availability'
  | 'blocked_by_firewall'
  | 'connection_failed'
  | 'connections_failed'
  | 'cpu_percent'
  | 'CpuPercentage'
  | 'CpuUtilization'
  | 'HealthCheckStatus'
  | 'is_db_alive'
  | 'longest_query_time_sec'
  | 'longest_transaction_time_sec'
  | 'MemoryPercentage'
  | 'ResponseTime'
  | 'RestartCount'
  | 'SuccessE2ELatency'
  | 'UnhealthyHostCount'
