@description('Name of the resource that these alerts are being applied to.')
param resourceName string

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../baseMemoryPercentageAlert.bicep' = {
  name: '${resourceName}MemoryPercentageAlertModule'
  params: {
    resourceName: resourceName
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    metricName: 'memory_percent' 
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}