import { staticAverageLessThanHundred } from '../config.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alert '../staticMetricAlert.bicep' = {
  name: '${resourceName}AvailabilityAlertModule'
  params: {
    resourceName: resourceName
    resourceMetric: {
      resourceType: 'Microsoft.Storage/storageAccounts'
      metric: 'availability'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'availability'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
