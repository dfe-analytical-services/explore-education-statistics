import { ResourceNames } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

module applicationInsightsModule '../../components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    location: location
    appInsightsName: resourceNames.publicApi.appInsights
  }
}

output appInsightsName string = resourceNames.publicApi.appInsights
output appInsightsKey string = applicationInsightsModule.outputs.applicationInsightsKey
output appInsightsConnectionString string = applicationInsightsModule.outputs.applicationInsightsConnectionString
