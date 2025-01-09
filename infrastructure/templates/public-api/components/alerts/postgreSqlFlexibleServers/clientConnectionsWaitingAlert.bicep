import { dynamicMaxGreaterThan } from '../dynamicAlertConfig.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../dynamicMetricAlert.bicep' = {
  name: '${resourceName}ClientConnectionsAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
      metric: 'client_connections_waiting'
    }
    config: {
      ...dynamicMaxGreaterThan
      nameSuffix: 'client-connections'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
