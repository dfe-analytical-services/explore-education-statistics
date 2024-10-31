@export()
type evaluationFrequencyType = 'PT1M'

@export()
type windowSizeType = 'PT5M'

@export()
type resourceTypeType = 
  | 'Microsoft.Web/sites' 
  | 'Microsoft.Web/sites/slots'
  | 'Microsoft.DBforPostgreSQL/flexibleServers'
  | 'Microsoft.Sql/servers/databases'

@export()
type metricNameType = 'HealthCheckStatus' | 'blocked_by_firewall'

@export()
type operatorType = 'GreaterThan' | 'LessThan'

@export()
type timeAggregationType = 'Total' | 'Minimum'
