import { Severity } from '../types.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('The alert severity.')
param severity Severity = 'Warning'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../staticMetricAlert.bicep' = {
  name: '${resourceName}RestartsAlertModule'
  params: {
    alertName: '${resourceName}-restarts'
    resourceIds: [resourceId('Microsoft.App/containerApps', resourceName)]
    resourceType: 'Microsoft.App/containerApps'
    query: {
      metric: 'RestartCount'
      aggregation: 'Total'
      operator: 'GreaterThan'
      threshold: 0
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
