import { EvaluationFrequency, WindowSize, Severity } from '../types.bicep'

@description('Name of the resource that this alert is being applied to.')
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

param severity Severity = 'Critical'

module metricAlertModule '../staticMetricAlert.bicep' = [for name in resourceNames: {
  name: '${replace(name, '/', '-')}UnhealthyAlertModule'
  params: {
    alertName: '${replace(name, '/', '-')}Unhealthy'
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
  }
}]
