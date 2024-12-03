import { Severity } from '../types.bicep'

param storageAccountNames string[]

param alertsGroupName string

param severity Severity = 'Critical'

module alerts '../staticMetricAlert.bicep' = [for name in storageAccountNames: {
  name: '${name}StorageAvailabilityAlertModule'
  params: {
    alertName: '${name}Availability'
    resourceIds: [resourceId('Microsoft.Storage/storageAccounts', name)]
    resourceType: 'Microsoft.Storage/storageAccounts'
    query: {
      metric: 'availability'
      aggregation: 'Average'
      operator: 'LessThan'
      threshold: 100
    }
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    severity: severity
    alertsGroupName: alertsGroupName
  }
}]
