import { abbreviations } from '../../common/abbreviations.bicep'
import { ResourceNames } from '../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

// Shared log analytics workspace. Currently the Public API deployment is responsible for creating this.
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: resourceNames.existingResources.logAnalyticsWorkspace
}

module applicationInsightsModule '../../public-api/components/appInsights.bicep' = {
  name: 'applicationInsightsModuleDeploy'
  params: {
    location: location
    appInsightsName: '${resourcePrefix}-${abbreviations.insightsComponents}-search'
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    alerts: {
      exceptionCount: true
      exceptionServerCount: true
      failedRequests: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    tagValues: tagValues
  }
}

output applicationInsightsConnectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
output applicationInsightsName string = applicationInsightsModule.outputs.applicationInsightsName
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
