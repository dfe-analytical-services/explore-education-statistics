import { staticAverageGreaterThanZero } from '../staticAlertConfig.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../staticMetricAlert.bicep' = {
  name: '${resourceName}BackendHealthAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.Network/applicationGateways'
      metric: 'UnhealthyHostCount'
    }
    config: {
      ...staticAverageGreaterThanZero
      nameSuffix: 'backend-pool-health'
      threshold: '0.05'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
