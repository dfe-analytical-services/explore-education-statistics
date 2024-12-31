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
  | 'client_connections_waiting'
  | 'connection_failed'
  | 'connections_failed'
  | 'cpu_percent'
  | 'CpuPercentage'
  | 'CpuUtilization'
  | 'disk_bandwidth_consumed_percentage'
  | 'disk_iops_consumed_percentage'
  | 'HealthCheckStatus'
  | 'is_db_alive'
  | 'longest_query_time_sec'
  | 'longest_transaction_time_sec'
  | 'memory_percent' 
  | 'MemoryPercentage'
  | 'ResponseTime'
  | 'RestartCount'
  | 'SuccessE2ELatency'
  | 'UnhealthyHostCount'

@export()
type DynamicAlertConfig = {
  @description('Suffix to append to the resource name in order to create an alert name.')
  nameSuffix: string
  
  @description('The aggregation applied to the metric values within the specified time window.')
  aggregation: TimeAggregation
  
  @description('The operator used to compare the aggregated metrics against the dynamic threshold.')
  operator: DynamicMetricOperator

  @description('''
  The frequency with which this alert rule evaluates the metrics against the dynamic thresholds.
  For instance, PT1M with a window size of PT5M will evaluate the past 5 minutes' worth of metric data
  against the dynamic threshold every minute.
  ''')
  evaluationFrequency: EvaluationFrequency

  @description('''
  The timespan that is used to calculate the metric's value against the specified time aggregation.
  For instance, PT5M with a time aggregation of "Average" will use 5 minutes of metric data to calculate
  the average value, which is then compared to the dynamic threshold.
  ''')
  windowSize: WindowSize
  
  @description('''
  How many periods to look back over to count failing periods.  Used in conjunction with "minFailingPeriodsToAlert".
  As an example, if "numberOfEvaluationPeriods" is set to 5 and "evaluationFrequency" is set to every minute, the past 
  5 alerts (one for each of the last 5 minutes) is looked at and each failure is counted up. 
  ''')
  numberOfEvaluationPeriods: int
  
  @description('''
  How many of the "numberOfEvaluationPeriods" results need to have failed in order for this rule to fire.
  For instance, if this rule is using the past 5 calculations (with "numberOfEvaluationPeriods" set to 5) to evaluate
  whether or not to fire, "minFailingPeriodsToAlert" determines how many of those past 5 periods have to have failed 
  in order for this rule to fire.  If this was set to 3, 3 out of the 5 past calculations will have had to fail in
  order for this rule to fire.
  ''')
  minFailingPeriodsToAlert: int

  @description('''
  How sensitive the alert is if a metric exceeds its dynamic threshold.
  Low sensitivity means that this alert will fire only if the metric exceeds the threshold by a high degree.
  High sensitivity means that this alert will fire if the metric exceeds the threshold to a much lower degree.  
  ''')
  sensitivity: Sensitivity

  @description('The alert severity.')
  severity: Severity
}
