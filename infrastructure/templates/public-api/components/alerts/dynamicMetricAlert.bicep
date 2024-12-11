import { EvaluationFrequency, MetricName, DynamicMetricOperator, ResourceType, TimeAggregation, WindowSize, Severity, Sensitivity, severityMapping } from 'types.bicep'

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

@description('The evaluation frequency.')
param evaluationFrequency EvaluationFrequency = 'PT1M'

@description('The window size.')
param windowSize WindowSize = 'PT5M'

@description('The alert severity.')
param severity Severity = 'Error'

param sensitivity Sensitivity = 'High'

param minFailingPeriodsToAlert int = 1

param numberOfEvaluationPeriods int = 1

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
      allOf: [{
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
      }]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
  tags: tagValues
}
