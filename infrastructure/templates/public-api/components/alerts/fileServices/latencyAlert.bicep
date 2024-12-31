import { Severity } from '../types.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('The alert severity.')
param severity Severity = 'Warning'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../dynamicMetricAlert.bicep' = {
  name: '${resourceName}FsLatencyAlertModule'
  params: {
    alertName: '${resourceName}-fileservice-latency'
    resourceIds: [resourceId('Microsoft.Storage/storageAccounts/fileServices', resourceName, 'default')]
    resourceType: 'Microsoft.Storage/storageAccounts/fileServices'
    query: {
      metric: 'SuccessE2ELatency'
      aggregation: 'Average'
      operator: 'GreaterThan'
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
