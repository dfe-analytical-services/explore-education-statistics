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

module unhealthyMetricAlertModule 'metricAlert.bicep' = {
  name: '${alertName}Deploy'
  params: {
    alertName: alertName
    alertsGroupName: alertsGroupName
    resourceId: resourceId
    resourceType: resourceType
    metricName: 'HealthCheckStatus'
    criterionType: 'StaticThresholdCriterion'
    operator: 'LessThan'
    threshold: 100
    timeAggregation: 'Minimum'
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
  }
}
