@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Function App : Runtime Language')
param functionAppRuntime string = 'dotnet'

@description('Specifies the name of the function.')
param functionAppName string = 'publicapi-processor'

@description('Storage Account connection string')
@secure()
param storageAccountConnectionString string

@description('Specifies the database connection string')
@secure()
param dbConnectionString string

@description('Specifies the service bus connection string.')
@secure()
param serviceBusConnectionString string

//Passed in Tags
param tagValues object


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
    settings: {
      dbConnectionString: dbConnectionString
      serviceBusConnectionString: serviceBusConnectionString
    }
    functionAppRuntime: functionAppRuntime
  }
}
