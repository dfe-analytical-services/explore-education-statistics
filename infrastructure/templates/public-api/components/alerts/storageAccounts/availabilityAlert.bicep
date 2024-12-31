import { Severity } from '../types.bicep'

@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('The alert severity.')
param severity Severity = 'Critical'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../staticMetricAlert.bicep' = [for name in resourceNames: {
  name: '${name}AvailabilityAlertModule'
  params: {
    alertName: '${name}-availability'
    resourceIds: [resourceId('Microsoft.Storage/storageAccounts', name)]
    resourceType: 'Microsoft.Storage/storageAccounts'
    query: {
      metric: 'availability'
      aggregation: 'Average'
      operator: 'LessThan'
      threshold: 100
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}]
