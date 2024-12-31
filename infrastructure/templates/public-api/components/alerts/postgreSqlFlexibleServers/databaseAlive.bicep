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
  name: '${resourceName}DbAliveAlertModule'
  params: {
    alertName: '${resourceName}-database-alive'
    resourceIds: [resourceId('Microsoft.DBforPostgreSQL/flexibleServers', resourceName)]
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    query: {
      metric: 'is_db_alive'
      aggregation: 'Minimum'
      operator: 'LessThan'
      threshold: 1
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
