import { resourceNamesType } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var logAnalyticsWorkspaceName = '${resourceNames.prefixes.common}-${resourceNames.abbreviations.operationalInsightsWorkspaces}'

module logAnalyticsWorkspaceModule '../../components/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    location: location
    tagValues: tagValues
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
