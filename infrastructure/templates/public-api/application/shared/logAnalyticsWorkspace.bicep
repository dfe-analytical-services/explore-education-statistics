import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

module logAnalyticsWorkspaceModule '../../components/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    logAnalyticsWorkspaceName: resourceNames.sharedResources.logAnalyticsWorkspace
    location: location
    tagValues: tagValues
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
