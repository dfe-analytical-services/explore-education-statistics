import { evaluationFrequencyType, metricNameType, operatorType, resourceTypeType, timeAggregationType, windowSizeType } from 'types.bicep'

@description('Name of the alert.')
param alertName string

@description('Id of the resource that this alert is being applied to.')
param resourceId string

@description('Type of the resource that this alert is being applied to.')
param resourceType resourceTypeType

@description('The metric that is being measured.')
param metricName metricNameType

@description('The time aggregation.')
param timeAggregation timeAggregationType

@description('The operator being used in the test.')
param operator operatorType

@description('Settings for implementing tests against a statically defined threshold.')
param staticThresholdSettings {
  threshold: int
}?

@description('Settings for implementing tests against a dynamic threshold defined through machine learning.')
param dynamicThresholdSettings {
  alertSensitivity: 'Low' | 'Medium' | 'High'
}?

@description('The evaluation frequency.')
param evaluationFrequency evaluationFrequencyType = 'PT1M'

@description('The window size.')
param windowSize windowSizeType = 'PT5M'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

resource alertsActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  name: alertsGroupName
}

var thresholdSettings = staticThresholdSettings != null 
? union({
  criterionType: 'StaticThresholdCriterion'
}, staticThresholdSettings!)
: union({
  criterionType: 'DynamicThresholdCriterion'
  failingPeriods: {
    minFailingPeriodsToAlert: 1
    numberOfEvaluationPeriods: 1
  }
}, dynamicThresholdSettings!)

resource metricAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: alertName
  location: 'Global'
  properties: {
    enabled: true
    scopes: [resourceId]
    severity: 1
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        union({
          name: 'Metric1'
          metricName: metricName
          timeAggregation: timeAggregation
          operator: operator
          skipMetricValidation: false
          metricNamespace: resourceType
        }, thresholdSettings)
      ]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
}
