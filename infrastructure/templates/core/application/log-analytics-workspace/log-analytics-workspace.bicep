@description('Name of the Log Analytics Workspace.')
param workspaceName string

@description('Specifies the location for the resource.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

module logAnalyticsWorkspaceModule '../../../common/components/log-analytics-workspace/log-analytics-workspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    logAnalyticsWorkspaceName: workspaceName
    location: location
    tagValues: tagValues
  }
}

output logAnalyticsWorkspaceId string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
