@description('Names of the resources that these alerts are being applied to.')
param resourceNames string[]

@description('Name of the Alerts Group used to send alert messages.')
param alertsGroupName string

@description('Tags with which to tag the resource in Azure.')
param tagValues object

module alerts '../baseCpuPercentageAlert.bicep' = {
  name: '${resourceNames[0]}CpuPercentageAlertModule'
  params: {
    resourceNames: resourceNames
    resourceType: 'Microsoft.DBforPostgreSQL/flexibleServers'
    metricName: 'cpu_percent' 
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
}
