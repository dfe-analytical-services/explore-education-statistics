var defaultDynamicAlertConfig = {
  evaluationFrequency: 'PT5M'
  evaluationPeriods: 5
  minFailingEvaluationPeriods: 5
  windowSize: 'PT15M'
  sensitivity: 'Low'
  severity: 'Warning'
}

@export()
var dynamicAverageGreaterThan = {
  ...defaultDynamicAlertConfig
  aggregation: 'Average'
  operator: 'GreaterThan'
}

@export()
var dynamicMaxGreaterThan = {
  ...defaultDynamicAlertConfig
  aggregation: 'Maximum'
  operator: 'GreaterThan'
}

@export()
var cpuPercentageConfig = union(dynamicAverageGreaterThan, {
  nameSuffix: 'cpu-percentage'
})

@export()
var memoryPercentageConfig = union(dynamicAverageGreaterThan, {
  nameSuffix: 'memory-percentage'
})

@export()
var responseTimeConfig = union(dynamicAverageGreaterThan, {
  nameSuffix: 'response-time'
})
