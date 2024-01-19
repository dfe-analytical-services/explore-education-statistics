@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Function App : Runtime Language')
param functionAppRuntime string = 'dotnet'

@description('Specifies the name of the function.')
param functionAppName string = 'processor'

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Database Connection String URI')
@secure()
param databaseConnectionStringURI string

@description('Storage Account Connection String')
param storageAccountConnectionString string

@description('Service Bus Connection String reference')
param serviceBusConnectionString string

//Passed in Tags
param tagValues object


// Variables and created data


//---------------------------------------------------------------------------------------------------------------
// All resources via modules
//---------------------------------------------------------------------------------------------------------------

//Function App Deployment
module functionAppModule '../components/functionApp.bicep' = {
  name: 'functionAppDeploy-${functionAppName}'
  params: {
    resourcePrefix: resourcePrefix
    functionAppName: functionAppName
    location: location
    tagValues: tagValues
    databaseConnectionString: databaseConnectionStringURI
    functionAppRuntime: functionAppRuntime
    storageAccountConnectionString: storageAccountConnectionString
    serviceBusConnectionString: serviceBusConnectionString
  }
}

//Key Vault Access Policy Deployment
module keyVaultAccessPolicy '../components/keyVaultAccessPolicy.bicep' = {
  name: 'keyVaultAccessPolicyDeploy-${functionAppName}'
  params: {
    keyVaultName: keyVaultName
    principalId: functionAppModule.outputs.principalId
    tenantId: functionAppModule.outputs.tenantId
  }
  dependsOn: [
    functionAppModule
  ]
}

