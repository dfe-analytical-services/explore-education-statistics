@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Specifies the name of the function.')
param functionAppName string

@description('Storage Account connection string')
@secure()
param storageAccountConnectionString string

@description('Specifies the database connection string')
@secure()
param dbConnectionString string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('Specifies the subnet id')
param subnetId string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

module functionAppModule '../components/functionApp.bicep' = {
  name: '${resourcePrefix}-${functionAppName}'
  params: {
    resourcePrefix: resourcePrefix
    functionAppName: functionAppName
    location: location
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsKey
    subnetId: subnetId
    settings: {
      dbConnectionString: dbConnectionString
      coreStorage: storageAccountConnectionString
    }
    functionAppRuntime: 'dotnet-isolated'
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
  }
}

output functionAppName string = functionAppModule.outputs.functionAppName
