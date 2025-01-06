var defaultDynamicAlertConfig = {
  aggregation: 'Average'
  operator: 'GreaterThan'
  evaluationFrequency: 'PT5M'
  evaluationPeriods: 5
  minFailingEvaluationPeriods: 5
  windowSize: 'PT15M'
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
