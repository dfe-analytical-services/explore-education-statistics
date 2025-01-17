import { TimeAggregation, EvaluationFrequency, WindowSize, Severity } from 'types.bicep'

@export()
type StaticAlertConfig = {
  @description('Suffix to append to the resource name in order to create an alert name.')
  nameSuffix: string

  @description('The aggregation applied to the metric values within the specified time window.')
  aggregation: TimeAggregation

  @description('The operator used to compare the aggregated metrics against the threshold.')
  operator: 'GreaterOrLessThan' | 'GreaterThan' | 'LessThan'

  @description('The static threshold against which the aggregated metric vaule will be tested.')
  threshold: string

  @description('''
  The frequency with which this alert rule evaluates the metrics against the threshold.
  For instance, PT1M with a window size of PT5M will evaluate the past 5 minutes' worth of metric data
  against the threshold every minute.
  ''')
  evaluationFrequency: EvaluationFrequency

  @description('''
  The timespan that is used to calculate the metric's value against the specified time aggregation.
  For instance, PT5M with a time aggregation of "Average" will use 5 minutes of metric data to calculate
  the average value, which is then compared to the threshold.
  ''')
  windowSize: WindowSize

  @description('The alert severity.')
  severity: Severity
}

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
var staticMinGreaterThanZero = {
  ...defaultStaticAlertConfig
  aggregation: 'Minimum'
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

@export()
var capacity = union(defaultStaticAlertConfig, {
  nameSuffix: 'capacity'
  windowSize: 'PT1H'
  aggregation: 'Average'
  operator: 'GreaterThan'
  severity: 'Warning'
  threshold: '85'
})
