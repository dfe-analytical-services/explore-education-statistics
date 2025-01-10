@export()
type EvaluationFrequency = 'PT1M' | 'PT5M' | 'PT15M' | 'PT30M' | 'PT1H'

@export()
type WindowSize = 'PT5M' | 'PT15M' | 'PT30M' | 'PT1H'

@export()
type TimeAggregation = 'Average' | 'Count' | 'Maximum' | 'Minimum' | 'Total'

@export()
type Severity = 'Critical' | 'Error' | 'Warning' | 'Informational' | 'Verbose'

@export()
type Sensitivity = 'High' | 'Medium' | 'Low'

@export()
var severityMapping = {
  Critical: 0
  Error: 1
  Warning: 2
  Informational: 3
  Verbose: 4
}

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
  
@export()
type DynamicAlertConfig = {
  @description('Suffix to append to the resource name in order to create an alert name.')
  nameSuffix: string

  @description('The aggregation applied to the metric values within the specified time window.')
  aggregation: TimeAggregation

  @description('The operator used to compare the aggregated metrics against the threshold.')
  operator: 'Equals' | 'GreaterThan' | 'GreaterThanOrEqual' | 'LessThan' | 'LessThanOrEqual'

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

  @description('''
  How many periods to look back over to count failing periods.  Used in conjunction with "minFailingEvaluationPeriods".
  As an example, if "evaluationPeriods" is set to 5 and "evaluationFrequency" is set to every minute, the past 
  5 alerts (one for each of the last 5 minutes) is looked at and each failure is counted up. 
  ''')
  evaluationPeriods: int

  @description('''
  How many of the "evaluationPeriods" results need to have failed in order for this rule to fire.
  For instance, if this rule is using the past 5 calculations (with "evaluationPeriods" set to 5) to evaluate
  whether or not to fire, "minFailingEvaluationPeriods" determines how many of those past 5 periods have to have failed 
  in order for this rule to fire.  If this was set to 3, 3 out of the 5 past calculations will have had to fail in
  order for this rule to fire.
  ''')
  minFailingEvaluationPeriods: int

  @description('''
  How sensitive the alert is if a metric exceeds its threshold.
  Low sensitivity means that this alert will fire only if the metric exceeds the threshold by a high degree.
  High sensitivity means that this alert will fire if the metric exceeds the threshold to a much lower degree.  
  ''')
  sensitivity: Sensitivity

  @description('The alert severity.')
  severity: Severity
}
