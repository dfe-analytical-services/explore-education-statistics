@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Specifies the Runtime Language of the function')
param functionAppRuntime string = 'dotnet'

@description('Specifies the name of the function.')
param functionAppName string

@description('Storage Account connection string')
@secure()
param storageAccountConnectionString string

@description('Specifies the database connection string')
@secure()
param dbConnectionString string

// @description('Specifies the service bus connection string.')
// @secure()
// param serviceBusConnectionString string

//Passed in Tags
param tagValues object

@description('Specifies the subnet id')
param subnetId string

param applicationInsightsKey string


// Variables and created data



//---------------------------------------------------------------------------------------------------------------
// All resources via modules
//---------------------------------------------------------------------------------------------------------------


//Function App Deployment
module functionAppModule '../components/functionApp.bicep' = {
  name: '${resourcePrefix}-${functionAppName}'
  params: {
    resourcePrefix: resourcePrefix
    functionAppName: functionAppName
    storageAccountConnectionString: storageAccountConnectionString
    location: location
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsKey
    subnetId: subnetId
    settings: {
      dbConnectionString: dbConnectionString
    }
    functionAppRuntime: functionAppRuntime
  }
}