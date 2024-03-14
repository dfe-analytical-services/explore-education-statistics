@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Application Insights name')
param appInsightsName string

var kind = 'web'
var insightsName = empty(appInsightsName)
  ? '${resourcePrefix}-ai'
  : '${resourcePrefix}-ai-${appInsightsName}'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: insightsName
  location: location
  kind: kind
  properties: {
    Application_Type: kind
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output applicationInsightsKey string = applicationInsights.properties.InstrumentationKey
