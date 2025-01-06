var defaultDynamicAlertConfig = {
  evaluationFrequency: 'PT5M'
  evaluationPeriods: 5
  minFailingEvaluationPeriods: 5
  windowSize: 'PT15M'
  sensitivity: 'Low'
  severity: 'Warning'
}

@export()
var greaterThanAverageDynamicConfig = {
  ...defaultDynamicAlertConfig
  aggregation: 'Average'
  operator: 'GreaterThan'
}

@export()
var greaterThanMaximumDynamicConfig = {
  ...defaultDynamicAlertConfig
  aggregation: 'Maximum'
  operator: 'GreaterThan'
}

@export()
var cpuPercentageConfig = union(greaterThanAverageDynamicConfig, {
  nameSuffix: 'cpu-percentage'
})

@export()
var memoryPercentageConfig = union(greaterThanAverageDynamicConfig, {
  nameSuffix: 'memory-percentage'
})

@export()
var responseTimeConfig = union(greaterThanAverageDynamicConfig, {
  nameSuffix: 'response-time'
})
