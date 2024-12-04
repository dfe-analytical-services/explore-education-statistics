import { EvaluationFrequency, WindowSize, Severity } from '../types.bicep'

@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('Type of the resource that this alert is being applied to.')
param resourceType 
  | 'Microsoft.Web/sites'
  | 'Microsoft.Web/sites/slots' = 'Microsoft.Web/sites'

@description('The evaluation frequency.')
param evaluationFrequency EvaluationFrequency = 'PT1M'

@description('The window size.')
param windowSize WindowSize = 'PT5M'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Severity level of the alert.')
param severity Severity = 'Critical'

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module metricAlertModule '../staticMetricAlert.bicep' = [for name in resourceNames: {
  name: '${replace(name, '/', '-')}HealthAlertModule'
  params: {
    alertName: '${replace(name, '/', '-')}-health'
    alertsGroupName: alertsGroupName
    resourceIds: [resourceId(resourceType, name)]
    resourceType: resourceType
    query: {
      metric: 'HealthCheckStatus'
      aggregation: 'Minimum'
      operator: 'LessThan'
      threshold: 100
    }
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    severity: severity
    tagValues: tagValues
  }
}]
