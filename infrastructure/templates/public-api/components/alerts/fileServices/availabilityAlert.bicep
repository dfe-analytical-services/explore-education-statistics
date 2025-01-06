import { Severity } from '../types.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('The alert severity.')
param severity Severity = 'Critical'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../staticMetricAlert.bicep' = {
  name: '${resourceName}FsAvailabilityAlertModule'
  params: {
    alertName: '${resourceName}-fileservice-availability'
    resourceIds: [resourceId('Microsoft.Storage/storageAccounts/fileServices', resourceName, 'default')]
    resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
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
}
