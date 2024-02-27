//Environment Params -------------------------------------------------------------------
@description('Environment: Subscription Name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = 's101d01'

@description('Environment: Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

//Tagging Params ------------------------------------------------------------------------
@description('Tagging: Environment Name e.g. Development. Used for tagging Resources for each environment.')
param environmentName string = 'Development'

@description('Storage : Size of the file share in GB.')
param fileShareQuota int = 1

@description('Storage: Specifies the full name (minus the subscription) of the pre-existing core Storage Account.')
param storageAccountName string = 'saeescore'

// PostgreSQL Database Params ------------------------------------------------------------------
@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string

@description('Database : administrator password.')
@minLength(8)
@secure()
param postgreSqlAdminPassword string

@description('Database : Azure Database for PostgreSQL sku name.')
@allowed([
  'Standard_B1ms'
  'Standard_D4ads_v5'
  'Standard_E4ads_v5'
])
param postgreSqlSkuName string = 'Standard_B1ms'

@description('Database : Azure Database for PostgreSQL Storage Size in GB.')
param postgreSqlStorageSizeGB int = 32

@description('Database : Azure Database for PostgreSQL Autogrow setting.')
param postgreSqlAutoGrowStatus string = 'Disabled'

@description('Database : Firewall rules.')
param postgreSqlFirewallRules array = []

@description('Container App : Select if you want to use a public dummy image to start the container app.')
param useDummyImage bool = true

param now string = utcNow('u')

//---------------------------------------------------------------------------------------------------------------
// Variables and created data.
//---------------------------------------------------------------------------------------------------------------
var project = 'ees-publicapi'
var resourcePrefix = '${subscription}-${project}'
var storageAccountFullName = '${subscription}${storageAccountName}'
var keyVaultFullName = '${subscription}-${keyVaultName}'
var containerAppName = 'api'
var containerAppImageName = useDummyImage ? 'azuredocs/aci-helloworld' : 'real-container-image-name'
var containerAppTargetPort = 80
var rootFileShareFolderName = 'data'
var containerRegistryName = 'eesacr'
var keyVaultName = 'kv-ees-01'
var databaseNames = ['publicapi']

var tagValues = {
  departmentName: 'Unknown'
  environmentName: environmentName
  solutionName: 'API'
  subscriptionName: 'Unknown'
  costCentre: 'Unknown'
  serviceOwnerName: 'Unknown'
  dateProvisioned: now
  createdBy: 'Unknown'
  deploymentRepo: 'N/A'
  deploymentScript: 'main.bicep'
}

//---------------------------------------------------------------------------------------------------------------
// All pre-existing resources managed by main ARM template.
//---------------------------------------------------------------------------------------------------------------

// Reference existing Key Vault.
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultFullName
  scope: resourceGroup(resourceGroup().name)
}

// Reference existing Azure Container Registry.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: containerRegistryName
}

// Reference existing core Storage Account.
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountFullName
  scope: resourceGroup(resourceGroup().name)
}
var storageAccountKey = storageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${storageAccountKey}'


//---------------------------------------------------------------------------------------------------------------
// All new resources via modules.
//---------------------------------------------------------------------------------------------------------------

module vNetModule 'application/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    subscription: subscription
    resourcePrefix: resourcePrefix
  }
}

// Deploy shared Application Insights for all relevant Public API resources to use.
module applicationInsightsModule 'components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    appInsightsName: ''
  }
}

// Deploy File Share.
module fileShareModule 'components/fileShares.bicep' = {
  name: 'fileShareDeploy'
  params: {
    resourcePrefix: resourcePrefix
    fileShareName: rootFileShareFolderName
    fileShareQuota: fileShareQuota
    storageAccountName: storageAccountFullName
  }
}

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    serverName: ''
    adminName: postgreSqlAdminName
    adminPassword: postgreSqlAdminPassword
    dbSkuName: postgreSqlSkuName
    dbStorageSizeGB: postgreSqlStorageSizeGB
    dbAutoGrowStatus: postgreSqlAutoGrowStatus
    keyVaultName: keyVaultFullName
    tagValues: tagValues
    vNetId: vNetModule.outputs.vNetRef
    subnetId: vNetModule.outputs.postgreSqlSubnetRef
    firewallRules: postgreSqlFirewallRules
    databaseNames: databaseNames
  }
  dependsOn: [
    vNetModule
  ]
}

// Deploy main Public API Container App.
module containerAppModule 'components/containerApp.bicep' = {
  name: 'containerAppDeploy-api'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerAppName: containerAppName
    containerAppEnvName: containerAppName
    containerAppLogAnalyticsName: containerAppName
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: containerAppImageName
    containerAppTargetPort: containerAppTargetPort
    useDummyImage: useDummyImage
    dbConnectionString: keyVault.getSecret(postgreSqlServerModule.outputs.connectionStringSecretName)
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.apiContainerAppSubnetRef
  }
  dependsOn: [
    postgreSqlServerModule
    applicationInsightsModule
    vNetModule
  ]
}

// Deploy data-processing Function.
module dataProcessorFunctionAppModule 'application/dataProcessorFunctionApp.bicep' = {
  name: 'functionAppDeploy-data-processor'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    functionAppName: 'data-processor'
    storageAccountConnectionString: storageAccountConnectionString
    dbConnectionString: keyVault.getSecret(postgreSqlServerModule.outputs.connectionStringSecretName)
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
  }
  dependsOn: [
    postgreSqlServerModule
    applicationInsightsModule
    vNetModule
  ]
}

// Outputs.
output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output containerRegistryName string = containerRegistry.name
output metadataDatabaseRef string = postgreSqlServerModule.outputs.databaseRef
output managedIdentityName string = containerAppModule.outputs.managedIdentityName
