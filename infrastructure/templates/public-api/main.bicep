@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Storage : Size of the file share in GB.')
param fileShareQuota int = 1

@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string?

@description('Database : administrator password.')
@minLength(8)
@secure()
param postgreSqlAdminPassword string?

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
param postgreSqlFirewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[] = []

@description('Container App : Select if you want to use a public dummy image to start the container app.')
param useDummyImage bool = true

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

var resourcePrefix = '${subscription}-ees-publicapi'
var storageAccountName = '${subscription}saeescore'
var keyVaultName = '${subscription}-kv-ees-01'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

// Reference the existing Key Vault resource as currently managed by the EES ARM template.
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
  scope: resourceGroup(resourceGroup().name)
}

// Reference the existing Azure Container Registry resource as currently managed by the EES ARM template.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: 'eesacr'
}

// Reference the existing core Storage Account as currently managed by the EES ARM template.
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
  scope: resourceGroup(resourceGroup().name)
}
var storageAccountKey = storageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${storageAccountKey}'


// Reference the existing VNet as currently managed by the EES ARM template, and register new subnets for Bicep-controlled resources.
module vNetModule 'application/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    subscription: subscription
    resourcePrefix: resourcePrefix
  }
}

// Deploy a single shared Application Insights for all relevant Public API resources to use.
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
    fileShareName: 'data'
    fileShareQuota: fileShareQuota
    storageAccountName: storageAccountName
  }
}

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    adminName: postgreSqlAdminName!
    adminPassword: postgreSqlAdminPassword!
    dbSkuName: postgreSqlSkuName
    dbStorageSizeGB: postgreSqlStorageSizeGB
    dbAutoGrowStatus: postgreSqlAutoGrowStatus
    keyVaultName: keyVaultName
    tagValues: tagValues
    vNetId: vNetModule.outputs.vNetRef
    firewallRules: postgreSqlFirewallRules
    subnetId: vNetModule.outputs.postgreSqlSubnetRef
    databaseNames: ['public_data']
  }
  dependsOn: [
    vNetModule
  ]
}

// Deploy main Public API Container App.
module apiContainerAppModule 'components/containerApp.bicep' = {
  name: 'apiContainerAppDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerAppName: 'api'
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: useDummyImage ? 'azuredocs/aci-helloworld' : 'real-container-image-name'
    containerAppTargetPort: 80
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
  name: 'dataProcessorFunctionAppDeploy'
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
