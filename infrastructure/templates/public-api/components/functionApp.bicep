@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Function App name')
param functionAppName string

@description('Function App Plan : operating system')
@allowed([
  'Windows'
  'Linux'
])
param appServicePlanOS string = 'Windows'

@description('Function App runtime')
@allowed([
  'dotnet'
  'node'
  'python'
  'java'
])
param functionAppRuntime string = 'dotnet'

@description('Storage Account connection string')
@secure()
param storageAccountConnectionString string

@description('Key Vault URI Connection String reference')
@secure()
param databaseConnectionString string

@description('Key Vault URI Connection String reference')
@secure()
param serviceBusConnectionString string

//Passed in Tags
param tagValues object

// Variables and created data
var kind = 'functionapp'
var databaseConnectionStringKeyVaultRef = '@Microsoft.KeyVault(SecretUri=${databaseConnectionString})'
var appServicePlanName = '${resourcePrefix}-aps-${functionAppName}'
var reserved = appServicePlanOS == 'Linux' ? true : false
var applicationInsightsName ='${resourcePrefix}-ai-${functionAppName}'
var functionName = '${resourcePrefix}-fa-${functionAppName}'

//Resources

//Application Insights Deployment
module applicationInsightsModule '../components/appInsights.bicep' = {
  name: 'appInsightsDeploy-${functionAppName}'
  params: {
    location: location
    appInsightsName: applicationInsightsName
  }
}

//App Service Plan Deployment
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  kind: kind
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
    capacity: 0
  }
  properties: {
    reserved: reserved
  }
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionName
  location: location
  kind: kind
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    clientAffinityEnabled: true
    reserved: true
  }
  tags: tagValues
  dependsOn: [
    applicationInsightsModule
  ]
}

resource functionAppSettings 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: {
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(functionAppName)
    FUNCTIONS_EXTENSION_VERSION: '~4'
    APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsightsModule.outputs.applicationInsightsKey
    FUNCTIONS_WORKER_RUNTIME: functionAppRuntime
    WEBSITE_RUN_FROM_PACKAGE: '1'
    DatabaseConnectionString: databaseConnectionStringKeyVaultRef
    ServiceBusConnectionString: serviceBusConnectionString
  }
}


//Output
output functionAppName string = functionApp.name
output principalId string = functionApp.identity.principalId
output tenantId string = functionApp.identity.tenantId
