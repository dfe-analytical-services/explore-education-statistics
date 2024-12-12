@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../baseResponseTimeAlert.bicep' = {
  name: '${resourceNames[0]}ResponseTimeAlertModule'
  params: {
    resourceNames: resourceNames
    resourceType: 'Microsoft.Network/applicationGateways'
    metricName: 'ApplicationGatewayTotalTime' 
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
