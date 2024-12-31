import { Severity } from '../types.bicep'

@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('The alert severity.')
param severity Severity = 'Critical'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../staticMetricAlert.bicep' = {
  name: '${resourceName}BackendHealthAlertModule'
  params: {
    alertName: '${resourceName}-backend-pool-health'
    resourceIds: [resourceId('Microsoft.Network/applicationGateways', resourceName)]
    resourceType: 'Microsoft.Network/applicationGateways'
    query: {
      metric: 'UnhealthyHostCount'
      aggregation: 'Average'
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
