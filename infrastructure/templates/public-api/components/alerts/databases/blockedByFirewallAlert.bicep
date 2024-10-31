import { evaluationFrequencyType, windowSizeType } from '../types.bicep'

@description('Name of the alert.')
param alertName string

@description('Id of the resource that this alert is being applied to.')
param resourceId string

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

module metricAlertModule '../metricAlert.bicep' = {
  name: '${alertName}Deploy'
  params: {
    alertName: alertName
    alertsGroupName: alertsGroupName
    resourceId: resourceId
    resourceType: resourceType
    metricName: 'blocked_by_firewall'
    criterionType: 'StaticThresholdCriterion'
    operator: 'GreaterThan'
    threshold: 0
    timeAggregation: 'Total'
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
  }
}
