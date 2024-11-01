import { evaluationFrequencyType, windowSizeType } from '../types.bicep'

@description('Id of the resource that this alert is being applied to.')
param resourceId string

@description('Name of the resource that this alert is being applied to.')
param resourceName string

@description('Type of the resource that this alert is being applied to.')
param resourceType 
  | 'Microsoft.DBforPostgreSQL/flexibleServers'
  | 'Microsoft.Sql/servers/databases'

@description('The evaluation frequency.')
param evaluationFrequency evaluationFrequencyType = 'PT1M'

@description('The window size.')
param windowSize windowSizeType = 'PT5M'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

var alertName = '${replace(resourceName, '/', '-')}FailedConnections'

module metricAlertModule '../staticMetricAlert.bicep' = {
  name: alertName
  params: {
    alertName: alertName
    alertsGroupName: alertsGroupName
    resourceId: resourceId
    resourceType: resourceType
    metricName: resourceType == 'Microsoft.DBforPostgreSQL/flexibleServers' 
      ? 'connections_failed' 
      : 'connection_failed'
    operator: 'GreaterThan'
    timeAggregation: 'Total'
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    threshold: 0
  }
}
