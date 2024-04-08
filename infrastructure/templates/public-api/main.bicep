@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Storage : Size of the file share in GB.')
param fileShareQuota int = 1

@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string = ''

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

@description('The tags of the Docker images to deploy.')
param dockerImagesTag string = ''

@description('Has the user-assigned Managed Identity for the API Container App been created and been assigned the AcrPull role yet?')
param apiContainerAppUserCreatedWithAcrPull bool = true

@description('Have database users been added to PSQL yet for Container App and Function App?')
param psqlDbUsersAdded bool = true

@description('Public URLs of other components in the service.')
param publicUrls {
  contentApi: string
}?

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool = false

var resourcePrefix = '${subscription}-ees-papi'
var apiContainerAppName = 'api'
var apiContainerAppManagedIdentityName = '${resourcePrefix}-id-${apiContainerAppName}'
var adminAppServiceFullName = '${subscription}-as-ees-admin'
var dataProcessorFunctionAppName = 'processor'
var dataProcessorFunctionAppFullName = '${resourcePrefix}-fa-${dataProcessorFunctionAppName}'
var psqlServerName = 'psql-flexibleserver'
var coreStorageAccountName = '${subscription}saeescore'
var keyVaultName = '${subscription}-kv-ees-01'
var acrName = 'eesacr'
var vNetName = '${subscription}-vnet-ees'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

// Reference the existing Azure Container Registry resource as currently managed by the EES ARM template.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

// Reference the existing core Storage Account as currently managed by the EES ARM template.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: coreStorageAccountName
  scope: resourceGroup(resourceGroup().name)
}
var storageAccountKey = storageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var coreStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${storageAccountKey}'

// Reference the existing VNet as currently managed by the EES ARM template, and register new subnets for Bicep-controlled resources.
module vNetModule 'application/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    vNetName: vNetName
    resourcePrefix: resourcePrefix
    subscription: subscription
    dataProcessorFunctionAppName: dataProcessorFunctionAppName
    postgreSqlServerName: psqlServerName
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

// Create a generic, shared Log Analytics Workspace for any relevant resources to use.
module logAnalyticsWorkspaceModule 'components/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    subscription: subscription
    location: location
    tagValues: tagValues
  }
}

// Create a generic Container App Environment for any Container Apps to use.
module containerAppEnvironmentModule 'components/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    subscription: subscription
    location: location
    containerAppEnvironmentNameSuffix: '01'
    subnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    tagValues: tagValues
  }
}

// Deploy File Share.
module fileShareModule 'components/fileShares.bicep' = {
  name: 'fileShareDeploy'
  params: {
    resourcePrefix: resourcePrefix
    fileShareName: 'data'
    fileShareQuota: fileShareQuota
    storageAccountName: coreStorageAccountName
  }
}

// In order to link PostgreSQL Flexible Server to a VNet, it must have a Private DNS zone available with a name ending
// with "postgres.database.azure.com".
resource postgreSqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'private.postgres.database.azure.com'
  location: 'global'
  resource vNetLink 'virtualNetworkLinks' = {
    name: '${subscription}-ees-${psqlServerName}-vnet-link'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vNetModule.outputs.vNetRef
      }
    }
  }
}

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    resourcePrefix: '${subscription}-ees'
    location: location
    createMode: 'Default'
    adminName: postgreSqlAdminName
    adminPassword: postgreSqlAdminPassword!
    dbSkuName: postgreSqlSkuName
    dbStorageSizeGB: postgreSqlStorageSizeGB
    dbAutoGrowStatus: postgreSqlAutoGrowStatus
    postgreSqlVersion: '16'
    tagValues: tagValues
    privateDnsZoneId: postgreSqlPrivateDnsZone.id
    /*
    TODO EES-5052 - temporarily disconnecting PostgreSQL Flexible Server from VNet integration whilst awaiting
    Security Group guidance on accessing resources behind VNet protection. Replacing for now with public access
    but only on specific subnets.
    */
    firewallRules: concat(postgreSqlFirewallRules, [
      {
        name: '${resourcePrefix}-ca-${apiContainerAppName}-subnet'
        startIpAddress: vNetModule.outputs.containerAppEnvironmentSubnetStartIpAddress
        endIpAddress: vNetModule.outputs.containerAppEnvironmentSubnetEndIpAddress
      }
      {
        name: '${dataProcessorFunctionAppFullName}-subnet'
        startIpAddress: vNetModule.outputs.dataProcessorSubnetStartIpAddress
        endIpAddress: vNetModule.outputs.dataProcessorSubnetEndIpAddress
      }
    ])
    // subnetId: vNetModule.outputs.postgreSqlSubnetRef
    databaseNames: ['public_data']
  }
}

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (apiContainerAppUserCreatedWithAcrPull) {
  name: apiContainerAppManagedIdentityName
}

// Deploy main Public API Container App.
module apiContainerAppModule 'components/containerApp.bicep' = if (apiContainerAppUserCreatedWithAcrPull && psqlDbUsersAdded) {
  name: 'apiContainerAppDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerAppName: apiContainerAppName
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: 'ees-public-api/api:${dockerImagesTag}'
    managedIdentityName: apiContainerAppManagedIdentity.name
    managedEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    appSettings: [
      {
        name: 'ConnectionStrings__PublicDataDb'
        value: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', apiContainerAppManagedIdentityName)
      }
      {
        // This settings allows the Container App to identify which user-assigned identity it should use in order to
        // perform Managed Identity-based authentication and authorization with other Azure services / resources.
        //
        // It is used in conjunction with the Azure.Identity .NET library to retrieve access tokens for the user-assigned
        // identity.
        name: 'AZURE_CLIENT_ID'
        value: apiContainerAppManagedIdentity.properties.clientId
      }
      {
        name: 'ContentApi__Url'
        value: publicUrls!.contentApi
      }
      {
        // This property informs the Container App of the name of the Admin's system-assigned identity.
        // It uses this to grant permissions to the Admin user in order for it to be able to access
        // tables in the "public_data" database successfully.
        name: 'AdminAppServiceIdentityName'
        value: adminAppServiceFullName
      }
      {
        // This property informs the Container App of the name of the Data Processor's system-assigned identity.
        // It uses this to grant permissions to the Data Processor user in order for it to be able to access
        // tables in the "public_data" database successfully.
        name: 'DataProcessorFunctionAppIdentityName'
        value: dataProcessorFunctionAppFullName
      }
    ]
    tagValues: tagValues
  }
}

// Deploy Data Processor Function.
module dataProcessorFunctionAppModule 'components/functionApp.bicep' = {
  name: 'dataProcessorFunctionAppDeploy'
  params: {
    subscription: subscription
    resourcePrefix: resourcePrefix
    functionAppName: dataProcessorFunctionAppName
    location: location
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
    functionAppExists: dataProcessorFunctionAppExists
    keyVaultName: keyVaultName
    functionAppRuntime: 'dotnet-isolated'
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    preWarmedInstanceCount: 1
  }
}

var dataProcessorPsqlConnectionStringSecretKey = 'dataProcessorPsqlConnectionString'

module storeDataProcessorPsqlConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storeDataProcessorPsqlConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: dataProcessorPsqlConnectionStringSecretKey
    secretValue: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', dataProcessorFunctionAppFullName)
    contentType: 'text/plain'
  }
}

var coreStorageConnectionStringSecretKey = 'coreStorageConnectionString'

module storeCoreStorageConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storeCoreStorageConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: coreStorageConnectionStringSecretKey
    secretValue: coreStorageConnectionString
    contentType: 'text/plain'
  }
}

output dataProcessorPsqlConnectionStringSecretKey string = dataProcessorPsqlConnectionStringSecretKey
output coreStorageConnectionStringSecretKey string = coreStorageConnectionStringSecretKey
