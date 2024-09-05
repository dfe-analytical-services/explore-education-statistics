@description('Specifies the location for all resources.')
param location string

@description('Specifies common resource prefix')
param commonResourcePrefix string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var logAnalyticsWorkspaceName = '${commonResourcePrefix}-log'

module logAnalyticsWorkspaceModule '../../components/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    location: location
    tagValues: tagValues
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
