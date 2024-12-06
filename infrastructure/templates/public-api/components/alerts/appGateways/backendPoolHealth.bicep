import { Severity } from '../types.bicep'

@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('The alert severity.')
param severity Severity = 'Critical'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../staticMetricAlert.bicep' = [for name in resourceNames: {
  name: '${name}BackendHealthAlertModule'
  params: {
    alertName: '${name}-backend-pool-health'
    resourceIds: [resourceId('Microsoft.Network/applicationGateways', name)]
    resourceType: 'Microsoft.Network/applicationGateways'
    query: {
      metric: 'UnhealthyHostCount'
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
}]
