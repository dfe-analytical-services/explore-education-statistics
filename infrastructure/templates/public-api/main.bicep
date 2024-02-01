//Environment Params -------------------------------------------------------------------
@description('Base domain name for Public API')
param domain string

@description('Subscription Name e.g. s101. Used as a prefix for created resources')
param subscription string

@description('Environment Name Used as a prefix for created resources')
param environment string = 'ees'

@description('Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

//Tagging Params ------------------------------------------------------------------------
param environmentName string = 'Development'

param tagValues object = {
  departmentName: 'Unknown'
  environmentName: environmentName
  solutionName: 'API'
  subscriptionName: 'Unknown'
  costCentre: 'Unknown'
  serviceOwnerName: 'Unknown'
  dateProvisioned: utcNow('u')
  createdBy: 'Unknown'
  deploymentRepo: 'N/A'
  deploymentScript: 'main.bicep'
}

//Networking Params --------------------------------------------------------------------------
@description('Networking : Deploy subnets for networking')
param deploySubnets bool = false

@description('Networking : Included whitelist')
param storageFirewallRules array = ['0.0.0.0/0']

//Storage Params ------------------------------------------------------------------------------
@description('Storage Account Name')
param storageAccountName string = 'core'

@description('Storage : Name of the root fileshare folder name')
@minLength(3)
@maxLength(63)
param fileShareName string = 'data'

@description('Storage : Type of the file share quota')
param fileShareQuota int = 1

//PostgreSQL Database Params -------------------------------------------------------------------
@description('Database : PostgreSQL server Name')
param dbServerName string = 'publicapi-data'

@description('Database : administrator login name')
@minLength(0)
param dbAdminName string

@description('Database : administrator password')
@minLength(8)
@secure()
param dbAdminPassword string

@description('Database : Azure Database for PostgreSQL sku name ')
@allowed([
  'Standard_B1ms'
  'Standard_D4ads_v5'
  'Standard_E4ads_v5'
])
param dbSkuName string = 'Standard_B1ms'

@description('Database : Azure Database for PostgreSQL Storage Size in GB')
param dbStorageSizeGB int = 32

@description('Database : Azure Database for PostgreSQL Autogrow setting')
param dbAutoGrowStatus string = 'Disabled'

//Container Registry Params ----------------------------------------------------------------
@minLength(5)
@maxLength(50)
@description('Registry : Name of the azure container registry (must be globally unique)')
param containerRegistryName string

@description('Deploy the Container Registry if you are not using an existing registry')
param deployRegistry bool

//Container App Params -------------------------------------------------------------------
@minLength(2)
@maxLength(32)
@description('Specifies the name of the container app.')
param containerAppName string

@description('Specifies the name of the container app environment.')
param containerAppEnvName string = 'publicapi'

@description('Specifies the name of the log analytics workspace.')
param containerAppLogAnalyticsName string = 'publicapi'

@description('Container App : Specifies the container image to deploy from the registry <name>:<tag>.')
param containerAppImageName string

@description('Specifies the container port.')
param containerAppTargetPort int = 80

@description('Select if you want to use a public dummy image to start the container app.')
param useDummyImage bool


//ServiceBus Queue Params -------------------------------------------------------------------
@description('Name of the Service Bus namespace')
param namespaceName string = 'processor'

@description('Name of the Queue')
param queueName string = 'Processorqueue'

//ETL Function Paramenters ------------------------------------------------------------------
@description('Specifies the name of the function.')
param functionAppName string = 'processor'



//---------------------------------------------------------------------------------------------------------------
// Variables and created data
//---------------------------------------------------------------------------------------------------------------
var resourcePrefix = '${subscription}-${environment}'
var reducedResourcePrefix = '${subscription}-api'


//---------------------------------------------------------------------------------------------------------------
// All resources via modules
//---------------------------------------------------------------------------------------------------------------

//Deploy Networking
module vnetModule 'components/network.bicep' = {
  name: 'virtualNetworkDeploy'
  params: {
    subscription: subscription
    environment: environment
    location: location
    deploySubnets: deploySubnets
    tagValues: tagValues
  }
}

//Deploy Key Vault
module keyVaultModule 'components/keyVault.bicep' = {
  name: 'keyVaultDeploy'
  params: {
    resourcePrefix: reducedResourcePrefix
    location: location
    tenantId: az.subscription().tenantId
    tagValues: tagValues
  }
}

resource eesKeyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultModule.outputs.keyVaultName
  scope: resourceGroup(resourceGroup().name)
}

//Deploy Storage Account
module storageAccountModule 'components/storageAccount.bicep' = {
  name: 'storageAccountDeploy'
  params: {
    resourcePrefix: reducedResourcePrefix
    location: location
    storageAccountName: storageAccountName
    storageSubnetRules: [vnetModule.outputs.adminSubnetRef, vnetModule.outputs.importerSubnetRef, vnetModule.outputs.publisherSubnetRef]
    storageFirewallRules: storageFirewallRules
    keyVaultName: keyVaultModule.outputs.keyVaultName
    tagValues: tagValues
  }
  dependsOn: [
    vnetModule
    keyVaultModule
  ]
}

//Deploy File Share
module fileShareModule 'components/fileShares.bicep' = {
  name: 'fileShareDeploy'
  params: {
    resourcePrefix: resourcePrefix
    fileShareName: fileShareName
    fileShareQuota: fileShareQuota
    storageAccountName: storageAccountModule.outputs.storageAccountName
  }
  dependsOn: [
    storageAccountModule
  ]
}


//Deploy PostgreSQL Database
module databaseModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    serverName: dbServerName
    adminName: dbAdminName
    adminPassword: dbAdminPassword
    dbSkuName: dbSkuName
    dbStorageSizeGB: dbStorageSizeGB 
    dbAutoGrowStatus: dbAutoGrowStatus
    keyVaultName: keyVaultModule.outputs.keyVaultName
    tagValues: tagValues
  }
  dependsOn: [
    vnetModule
    keyVaultModule
  ]
}

//Deploy Container Registry 
module containerRegistryModule 'components/containerRegistry.bicep' = {
  name: 'acrDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerRegistryName: containerRegistryName
    deployRegistry: deployRegistry
    tagValues: tagValues
  }
}

//Deploy Container Application
module containerAppModule 'components/containerApp.bicep' = {
  name: 'appContainerDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerAppName: containerAppName
    containerAppEnvName: containerAppEnvName
    containerAppLogAnalyticsName: containerAppLogAnalyticsName
    acrLoginServer: containerRegistryModule.outputs.containerRegistryLoginServer
    containerAppImageName: containerAppImageName
    containerAppTargetPort: containerAppTargetPort
    useDummyImage: useDummyImage
    dbConnectionString: eesKeyVault.getSecret(databaseModule.outputs.connectionStringSecretName)
    serviceBusConnectionString: eesKeyVault.getSecret(serviceBusFunctionQueueModule.outputs.connectionStringSecretName)
    tagValues: tagValues
  }
  dependsOn: [
    keyVaultModule
    databaseModule
    serviceBusFunctionQueueModule
  ]
}

//Deploy Service Bus
module serviceBusFunctionQueueModule 'components/serviceBusQueue.bicep' = {
  name: 'serviceBusQueueDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    namespaceName: namespaceName
    queueName:queueName
    keyVaultName: keyVaultModule.outputs.keyVaultName
    tagValues: tagValues
  }
  dependsOn: [
    keyVaultModule
  ]
}

//Deploy ETL Function
module etlFunctionAppModule 'application/processorFunctionApp.bicep' = {
  name: 'processorFunctionAppDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    functionAppName: functionAppName
    storageAccountConnectionString: eesKeyVault.getSecret(storageAccountModule.outputs.connectionStringSecretName)
    dbConnectionString: eesKeyVault.getSecret(databaseModule.outputs.connectionStringSecretName)
    serviceBusConnectionString: eesKeyVault.getSecret(serviceBusFunctionQueueModule.outputs.connectionStringSecretName)
    tagValues: tagValues
  }
  dependsOn: [
    keyVaultModule
    databaseModule
    storageAccountModule
    serviceBusFunctionQueueModule
  ]
}


//outputs
output containerRegistryLoginServer string = containerRegistryModule.outputs.containerRegistryLoginServer
output containerRegistryName string = containerRegistryModule.name
output metadataDatabaseRef string = databaseModule.outputs.databaseRef
output managedIdentityName string = containerAppModule.outputs.managedIdentityName
