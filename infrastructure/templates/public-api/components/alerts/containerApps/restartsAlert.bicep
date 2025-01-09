import { staticTotalGreaterThanZero } from '../config.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../staticMetricAlert.bicep' = {
  name: '${resourceName}RestartsAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'RestartCount'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'restarts'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
