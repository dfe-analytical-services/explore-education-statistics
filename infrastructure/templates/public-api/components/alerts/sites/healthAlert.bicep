import { staticAverageLessThanHundred } from '../staticAlertConfig.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../staticMetricAlert.bicep' = {
  name: '${resourceName}HealthAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.Web/sites'
      metric: 'HealthCheckStatus'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'health'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
