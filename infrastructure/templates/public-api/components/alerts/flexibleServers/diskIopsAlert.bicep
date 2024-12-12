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
  name: '${name}DiskIopsAlertModule'
  params: {
    alertName: '${name}-disk-bandwidth'
    resourceIds: [resourceId('Microsoft.DBforPostgreSQL/flexibleServers', name)]
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    query: {
      metric: 'disk_iops_consumed_percentage'
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
