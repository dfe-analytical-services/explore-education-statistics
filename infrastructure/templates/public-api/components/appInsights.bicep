
@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Application Insights name')
param appInsightsName string

// Variables and created data
var kind = 'web'


//Resources
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: kind
  properties: {
    Application_Type: kind
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}


//Output
output applicationInsightsKey string = applicationInsights.properties.InstrumentationKey
