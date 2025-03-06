import { ResourceNames } from '../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

module applicationInsightsModule '../../public-api/components/appInsights.bicep' = {
  name: 'applicationInsightsModuleDeploy'
  params: {
    location: location
    appInsightsName: resourceNames.search.applicationInsights
    alerts: {
      exceptionCount: true
      exceptionServerCount: true
      failedRequests: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    tagValues: tagValues
  }
}

output connectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
