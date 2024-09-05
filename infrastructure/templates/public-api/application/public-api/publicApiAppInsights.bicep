@description('Specifies the location for all resources.')
param location string

@description('Specifies the Public API resource prefix')
param publicApiResourcePrefix string

var appInsightsName = '${publicApiResourcePrefix}-ai'

module applicationInsightsModule '../../components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    location: location
    appInsightsName: appInsightsName
  }
}

output appInsightsName string = appInsightsName
output appInsightsKey string = applicationInsightsModule.outputs.applicationInsightsKey
