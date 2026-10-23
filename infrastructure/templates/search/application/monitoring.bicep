import { ResourceNames } from '../../common/resource-names.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

// Shared log analytics workspace. Currently the Public API deployment is responsible for creating this.
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: resourceNames.logAnalyticsWorkspace
}

module applicationInsightsModule '../../common/components/monitoring/appInsights.bicep' = {
  name: 'applicationInsightsModuleDeploy'
  params: {
    location: location
    appInsightsName: resourceNames.search.appInsights
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.id
    alerts: {
      exceptionCount: true
      exceptionServerCount: true
      failedRequests: true
      alertsGroupName: resourceNames.alertsGroup
    }
    tagValues: tagValues
  }
}

output applicationInsightsConnectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
output applicationInsightsName string = applicationInsightsModule.outputs.applicationInsightsName
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
