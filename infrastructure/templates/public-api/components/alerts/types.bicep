@export()
type EvaluationFrequency = 'PT1M'

@export()
type WindowSize = 'PT5M'

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
  | 'Microsoft.Web/sites' 
  | 'Microsoft.Web/sites/slots'

@export()
type MetricName = 
  | 'availability'
  | 'blocked_by_firewall'
  | 'connection_failed'
  | 'connections_failed'
  | 'cpu_percent'
  | 'HealthCheckStatus'
  | 'is_db_alive'
  | 'RestartCount'
  | 'UnhealthyHostCount'