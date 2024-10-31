import { evaluationFrequencyType, windowSizeType } from 'types.bicep'

@description('Name of the alert.')
param alertName string

@description('Id of the resource that this alert is being applied to.')
param resourceId string

@description('Type of the resource that this alert is being applied to.')
param resourceType 'Microsoft.Web/sites' | 'Microsoft.Web/sites/slots'

@description('The evaluation frequency.')
param evaluationFrequency evaluationFrequencyType = 'PT1M'

@description('The window size.')
param windowSize 'PT5M' = 'PT5M'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

resource alertsActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  name: alertsGroupName
}

resource functionAppUnhealthyMetricAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
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
        {
          name: 'Metric1'
          criterionType: 'StaticThresholdCriterion'
          metricName: 'HealthCheckStatus'
          timeAggregation: 'Minimum'
          operator: 'LessThan'
          threshold: 100
          skipMetricValidation: false
          metricNamespace: resourceType
        }
      ]
    }
    actions: [
      {
        actionGroupId: alertsActionGroup.id
      }
    ]
  }
}
