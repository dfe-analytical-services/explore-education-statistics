import { staticAverageLessThanHundred } from '../staticAlertConfig.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../staticMetricAlert.bicep' = {
  name: '${resourceName}DbAliveAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'is_db_alive'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'database-alive'
      threshold: '1'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
