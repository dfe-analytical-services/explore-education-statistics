@export()
type EvaluationFrequency = 'PT1M'

@export()
type WindowSize = 'PT5M'

@export()
type MetricOperator =
  | 'GreaterOrLessThan' 
  | 'GreaterThan'
  | 'LessThan'

@export()
type TimeAggregation = 
  | 'Total'
  | 'Minimum'
  | 'Average'

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
  | 'Microsoft.Web/sites' 
  | 'Microsoft.Web/sites/slots'
  | 'Microsoft.DBforPostgreSQL/flexibleServers'
  | 'Microsoft.Sql/servers/databases'
  | 'Microsoft.Storage/storageAccounts'
  | 'Microsoft.Network/applicationGateways'
  | 'Microsoft.App/containerApps'

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
