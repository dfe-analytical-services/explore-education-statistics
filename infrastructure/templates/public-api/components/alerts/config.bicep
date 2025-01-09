// Common dynamic alert configuration

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

// Common static alert configuration

var defaultStaticAlertConfig = {
  evaluationFrequency: 'PT5M'
  windowSize: 'PT15M'
  severity: 'Warning'
}

@export()
var staticAverageGreaterThanZero = {
  ...defaultStaticAlertConfig
  aggregation: 'Average'
  operator: 'GreaterThan'
  threshold: '0'
}

@export()
var staticAverageLessThanHundred = {
  ...defaultStaticAlertConfig
  aggregation: 'Average'
  operator: 'LessThan'
  threshold: '100'
}

@export()
var staticMaxGreaterThanZero = {
  ...defaultStaticAlertConfig
  aggregation: 'Maximum'
  operator: 'GreaterThan'
  threshold: '0'
}

@export()
var staticTotalGreaterThanZero = {
  ...defaultStaticAlertConfig
  aggregation: 'Total'
  operator: 'GreaterThan'
  threshold: '0'
}
