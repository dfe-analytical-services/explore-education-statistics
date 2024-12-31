var defaultDynamicAlertConfig = {
  aggregation: 'Average'
  operator: 'GreaterThan'
  evaluationFrequency: 'PT5M'
  windowSize: 'PT15M'
  numberOfEvaluationPeriods: 5
  minFailingPeriodsToAlert: 5
  sensitivity: 'Low'
  severity: 'Warning'
}

@export()
var cpuPercentageConfig = union(defaultDynamicAlertConfig, {
  nameSuffix: 'cpu-percentage'
})

@export()
var memoryPercentageConfig = union(defaultDynamicAlertConfig, {
  nameSuffix: 'memory-percentage'
})

@export()
var responseTimeConfig = union(defaultDynamicAlertConfig, {
  nameSuffix: 'response-time'
})
