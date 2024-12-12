import { Severity } from '../types.bicep'

@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('The alert severity.')
param severity Severity = 'Warning'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../dynamicMetricAlert.bicep' = [for name in resourceNames: {
  name: '${name}MemoryPercentageAlertModule'
  params: {
    alertName: '${name}-memory-percentage'
    resourceIds: [resourceId('Microsoft.Web/serverfarms', name)]
    resourceType: 'Microsoft.Web/serverfarms'
    query: {
      metric: 'MemoryPercentage'
      aggregation: 'Average'
      operator: 'GreaterThan'
    }
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}]
