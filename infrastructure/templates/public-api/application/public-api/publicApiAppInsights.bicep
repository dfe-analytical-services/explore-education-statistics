import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Environment name e.g. Development, used to give the availability test a clear, environment-specific name and description.')
param environmentName string

@description('Public URL of the Public API, used as the target of the availability test.')
param publicApiUrl string

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: resourceNames.existingResources.logAnalyticsWorkspace
}

module applicationInsightsModule '../../../common/components/monitoring/appInsights.bicep' = {
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

// Availability test that behaves like an anonymous external Public API consumer, calling a
// genuine, publication-list-independent, read-only endpoint that depends on both PostgreSQL and
// Azure AI Search. This detects when the public route, the Public API app, PostgreSQL or
// Azure AI Search prevent the request from completing successfully.
module availabilityTestModule '../../../common/components/monitoring/availability-test.bicep' = {
  name: 'publicApiAvailabilityTestDeploy'
  params: {
    name: '${resourceNames.publicApi.appInsights}-publications'
    location: location
    appInsightsId: applicationInsightsModule.outputs.applicationInsightsId
    testDescription: '${environmentName} Public API availability test - anonymous GET of the publications list'
    url: '${publicApiUrl}/v1/publications?pageSize=1'
    alertsGroupName: resourceNames.existingResources.alertsGroup
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

output appInsightsName string = resourceNames.publicApi.appInsights
output appInsightsKey string = applicationInsightsModule.outputs.applicationInsightsKey
output appInsightsConnectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
