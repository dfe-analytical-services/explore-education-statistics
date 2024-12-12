@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../baseMemoryPercentageAlert.bicep' = {
  name: '${resourceNames[0]}MemoryPercentageAlertModule'
  params: {
    resourceNames: resourceNames
    resourceType: 'Microsoft.Web/serverfarms'
    metricName: 'MemoryPercentage' 
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
