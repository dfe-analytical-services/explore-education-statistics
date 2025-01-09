import { dynamicAverageGreaterThan } from '../dynamicAlertConfig.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../dynamicMetricAlert.bicep' = {
  name: '${resourceName}DiskIopsAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'disk_iops_consumed_percentage'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'disk-iops'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
