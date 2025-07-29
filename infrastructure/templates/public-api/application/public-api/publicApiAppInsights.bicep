import { ResourceNames } from '../../types.bicep'

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

module applicationInsightsModule '../../components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    location: location
    appInsightsName: resourceNames.publicApi.appInsights
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

output appInsightsName string = resourceNames.publicApi.appInsights
output appInsightsKey string = applicationInsightsModule.outputs.applicationInsightsKey
output appInsightsConnectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
