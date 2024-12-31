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
  name: '${resourceName}QueryTimeAlertModule'
  params: {
    alertName: '${resourceName}-query-time'
    resourceIds: [resourceId('Microsoft.DBforPostgreSQL/flexibleServers', resourceName)]
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    query: {
      metric: 'longest_query_time_sec'
      aggregation: 'Maximum'
      operator: 'GreaterThan'
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
