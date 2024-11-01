import { evaluationFrequencyType, windowSizeType } from 'types.bicep'

@description('Id of the resource that this alert is being applied to.')
param resourceId string

@description('Name of the resource that this alert is being applied to.')
param resourceName string

@description('Type of the resource that this alert is being applied to.')
param resourceType 
  | 'Microsoft.Web/sites' 
  | 'Microsoft.Web/sites/slots'
  | 'Microsoft.DBforPostgreSQL/flexibleServers'

@description('The evaluation frequency.')
param evaluationFrequency evaluationFrequencyType = 'PT1M'

@description('The window size.')
param windowSize windowSizeType = 'PT5M'

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

var alertName = '${replace(resourceName, '/', '-')}CpuPercent'

module metricAlertModule 'metricAlert.bicep' = {
  name: '${alertName}Deploy'
  params: {
    alertName: alertName
    alertsGroupName: alertsGroupName
    resourceId: resourceId
    resourceType: resourceType
    metricName: 'cpu_percentage'
    operator: 'GreaterThan'
    timeAggregation: 'Average'
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    dynamicThresholdSettings: {
      alertSensitivity: 'Medium'
    }
  }
}
