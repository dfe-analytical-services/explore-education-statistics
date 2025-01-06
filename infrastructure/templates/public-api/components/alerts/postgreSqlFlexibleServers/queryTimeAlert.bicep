import { dynamicMaxGreaterThan } from '../config.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../dynamicMetricAlert.bicep' = {
  name: '${resourceName}QueryTimeAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'longest_query_time_sec'
    }
    config: {
      ...dynamicMaxGreaterThan
      nameSuffix: 'max-query-time'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
