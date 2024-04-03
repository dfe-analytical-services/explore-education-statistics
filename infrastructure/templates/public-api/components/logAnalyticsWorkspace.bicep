@description('Specifies the subscription name of the Log Analytics Workspace - used for creating name prefix')
param subscription string

@description('Specifies the location of the Log Analytics Workspace - defaults to that of the Resource Group')
param location string

@description('Specifies the SKU of the Log Analytics Workspace - defaults to "PerGB2018"')
param sku {
  name: string
} = {
  name: 'PerGB2018'
}

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

var logAnalyticsWorkspaceName = '${subscription}-ees-log'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: sku
  }
  tags: tagValues
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceName
