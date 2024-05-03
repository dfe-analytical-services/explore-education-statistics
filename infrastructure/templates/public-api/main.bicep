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

@description('Can we deploy the Container App yet?  This is dependent on the user-assigned Managed Identity for the API Container App being created with the AcrPull role, and the database users added to PSQL.')
param deployContainerApp bool = true

// TODO EES-5128 - Note that this has been added temporarily to avoid 10+ minute deploys where it appears that PSQL 
// will redeploy even if no changes exist in this deploy from the previous one.
@description('Does the PostgreSQL Flexible Server require any updates? False by default to avoid unnecessarily lengthy deploys.')
param updatePsqlFlexibleServer bool = false

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
var publisherFunctionAppFullName = '${subscription}-fa-ees-publisher'
var dataProcessorFunctionAppName = 'processor'
var dataProcessorFunctionAppFullName = '${resourcePrefix}-fa-${dataProcessorFunctionAppName}'
var psqlServerName = 'psql-flexibleserver'
var psqlServerFullName = '${subscription}-ees-${psqlServerName}'
var coreStorageAccountName = '${subscription}saeescore'
var keyVaultName = '${subscription}-kv-ees-01'
var acrName = 'eesacr'
var vNetName = '${subscription}-vnet-ees'
var containerAppEnvironmentNameSuffix = '01'

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
    dataProcessorFunctionAppNameSuffix: dataProcessorFunctionAppName
    containerAppEnvironmentNameSuffix: containerAppEnvironmentNameSuffix
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
    containerAppEnvironmentNameSuffix: containerAppEnvironmentNameSuffix
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

var formattedPostgreSqlFirewallRules = map(postgreSqlFirewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  startIpAddress: rule.startIpAddress
  endIpAddress: rule.endIpAddress
})

// TODO EES-5128 - if keeping the flag for the future, move this conditional logic into the postgresqlDatabase.bicep 
// module itself so that we always have a set of outputs that we can use. This will in turn allow us to move 
// psqlManagedIdentityConnectionStringTemplate into the module's outputs.
resource postgreSqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' existing = if (!updatePsqlFlexibleServer) {
  name: psqlServerFullName
}

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = if (updatePsqlFlexibleServer) {
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
    firewallRules: formattedPostgreSqlFirewallRules
    databaseNames: ['public_data']
  }
}

var psqlDomainName = '${psqlServerFullName}.postgres.database.azure.com'

var psqlManagedIdentityConnectionStringTemplate = 'Server=${psqlDomainName};Database=[database_name];Port=5432;User Id=[managed_identity_name];Password=[access_token]'

var psqlPrivateLinkDnsZoneName = 'privatelink.postgres.database.azure.com'

var psqlPrivateEndpointName = '${psqlServerFullName}-plink'

var psqlPrivateEndpointNetworkInterfaceName = '${psqlPrivateEndpointName}-nic'

resource psqlPrivateEndpoint 'Microsoft.Network/privateEndpoints@2021-05-01' = {
  name: psqlPrivateEndpointName
  location: location
  properties: {
    subnet: {
      id: vNetModule.outputs.psqlFlexibleServerSubnetRef
    }
    privateLinkServiceConnections: [
      {
        name: psqlPrivateEndpointName
        properties: {
          privateLinkServiceId: updatePsqlFlexibleServer ? postgreSqlServerModule.outputs.databaseRef : postgreSqlServer.id
          groupIds: [
            'postgresqlServer'
          ]
        }
      }
    ]
  }
}

resource psqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: psqlPrivateLinkDnsZoneName
  location: 'global'
  properties: {}
  dependsOn: [
    vNetModule
  ]
}

resource psqlPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: psqlPrivateDnsZone
  name: '${psqlPrivateLinkDnsZoneName}-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vNetModule.outputs.vNetRef
    }
  }
}

resource psqlPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: 'default'
  parent: psqlPrivateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: replace(psqlPrivateLinkDnsZoneName, '.', '-')
        properties: {
          privateDnsZoneId: psqlPrivateDnsZone.id
        }
      }
    ]
  }
}

// TODO EES-5128 - move into the Container App module?
resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (deployContainerApp) {
  name: apiContainerAppManagedIdentityName
}

// Deploy main Public API Container App.
module apiContainerAppModule 'components/containerApp.bicep' = if (deployContainerApp) {
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
        value: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', apiContainerAppManagedIdentityName)
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
        name: 'MiniProfiler__Enabled'
        value: 'true'
      }
      {
        name: 'ParquetFiles__BasePath'
        value: 'data/public-api-parquet'
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
      {
        // This property informs the Container App of the name of the Publisher's system-assigned identity.
        // It uses this to grant permissions to the Publisher user in order for it to be able to access
        // tables in the "public_data" database successfully.
        name: 'PublisherFunctionAppIdentityName'
        value: publisherFunctionAppFullName
      }
    ]
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
  ]
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

var dataProcessorPsqlConnectionStringSecretKey = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

module storeDataProcessorPsqlConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storeDataProcessorPsqlConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: dataProcessorPsqlConnectionStringSecretKey
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', dataProcessorFunctionAppFullName)
    contentType: 'text/plain'
  }
}

var publisherPsqlConnectionStringSecretKey = 'ees-publisher-connectionstring-publicdatadb'

module storePublisherPsqlConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storePublisherPsqlConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: publisherPsqlConnectionStringSecretKey
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', publisherFunctionAppFullName)
    contentType: 'text/plain'
  }
}

var adminPsqlConnectionStringSecretKey = 'ees-admin-connectionstring-publicdatadb'

module storeAdminPsqlConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storeAdminPsqlConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: adminPsqlConnectionStringSecretKey
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', adminAppServiceFullName)
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
output keyVaultName string = keyVaultName
