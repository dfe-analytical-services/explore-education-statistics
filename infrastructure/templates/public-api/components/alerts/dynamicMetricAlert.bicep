import {
  EvaluationFrequency
  MetricName
  DynamicMetricOperator
  ResourceType
  TimeAggregation
  WindowSize
  Severity
  Sensitivity
  severityMapping
} from 'types.bicep'

@description('Name of the alert.')
param alertName string

@description('Ids of the resources that this alert is being applied to.')
param resourceIds string[]

@description('Type of the resource that this alert is being applied to.')
param resourceType ResourceType

@description('The query being used to test if the alert should be fired.')
param query {
  metric: MetricName
  aggregation: TimeAggregation
  operator: DynamicMetricOperator
}

@description('''
The frequency with which this alert rule evaluates the metrics against the dynamic thresholds.
For instance, PT1M with a window size of PT5M will evaluate the past 5 minutes' worth of metric data
against the dynamic threshold every minute.
''')
param evaluationFrequency EvaluationFrequency = 'PT1M'

@description('''
The timespan that is used to calculate the metric's value against the specified time aggregation.
For instance, PT5M with a time aggregation of "Average" will use 5 minutes of metric data to calculate
the average value, which is then compared to the dynamic threshold.
''')
param windowSize WindowSize = 'PT5M'

@description('''
How sensitive the alert is if a metric exceeds its dynamic threshold.
Low sensitivity means that this alert will fire only if the metric exceeds the threshold by a high degree.
High sensitivity means that this alert will fire if the metric exceeds the threshold to a much lower degree.  
''')
param sensitivity Sensitivity = 'Low'

@description('''
How many periods to look back over to count failing periods.  Used in conjunction with "minFailingPeriodsToAlert".
As an example, if "numberOfEvaluationPeriods" is set to 5 and "evaluationFrequency" is set to every minute, the past 
5 alerts (one for each of the last 5 minutes) is looked at and each failure is counted up. 
''')
param numberOfEvaluationPeriods int = 5

@description('''
How many of the "numberOfEvaluationPeriods" results need to have failed in order for this rule to fire.
For instance, if this rule is using the past 5 calculations (with "numberOfEvaluationPeriods" set to 5) to evaluate
whether or not to fire, "minFailingPeriodsToAlert" determines how many of those past 5 periods have to have failed 
in order for this rule to fire.  If this was set to 3, 3 out of the 5 past calculations will have had to fail in
order for this rule to fire.
''')
param minFailingPeriodsToAlert int = 5

@description('The alert severity.')
param severity Severity = 'Error'

@description('''
An optional date that prevents machine learning algorithms from using metric data prior to this date in order to
calculate its dynamic threshold.
''')
param ignoreDataBefore string?

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

var severityLevel = severityMapping[severity]

resource alertsActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  name: alertsGroupName
}

resource metricAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: alertName
  location: 'Global'
  properties: {
    enabled: true
    scopes: resourceIds
    severity: severityLevel
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'DynamicThresholdCriterion'
          name: 'Metric1'
          metricName: query.metric
          metricNamespace: resourceType
          timeAggregation: query.aggregation
          operator: query.operator
          alertSensitivity: sensitivity
          skipMetricValidation: false
          failingPeriods: {
            minFailingPeriodsToAlert: minFailingPeriodsToAlert
            numberOfEvaluationPeriods: numberOfEvaluationPeriods
          }
          ignoreDataBefore: ignoreDataBefore
        }
      ]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
  tags: tagValues
}
