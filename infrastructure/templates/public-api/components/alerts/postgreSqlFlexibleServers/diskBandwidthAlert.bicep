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
  name: '${resourceName}DiskBandwidthAlertModule'
  params: {
    alertName: '${resourceName}-disk-bandwidth'
    resourceIds: [resourceId('Microsoft.DBforPostgreSQL/flexibleServers', resourceName)]
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    query: {
      metric: 'disk_bandwidth_consumed_percentage'
      aggregation: 'Average'
      operator: 'GreaterThan'
    }
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
