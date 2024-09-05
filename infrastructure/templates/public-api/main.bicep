@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Public API Storage : Size of the file share in GB.')
param fileShareQuota int = 1

@description('Public API Storage : Firewall rules.')
param storageFirewallRules {
  name: string
  cidr: string
}[] = []

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
  cidr: string
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

@description('Can we deploy the Container App yet? This is dependent on the PostgreSQL Flexible Server being set up and having users manually added.')
param deployContainerApp bool = true

// TODO EES-5128 - Note that this has been added temporarily to avoid 10+ minute deploys where it appears that PSQL
// will redeploy even if no changes exist in this deploy from the previous one.
@description('Does the PostgreSQL Flexible Server require any updates? False by default to avoid unnecessarily lengthy deploys.')
param updatePsqlFlexibleServer bool = false

@description('Public URLs of other components in the service.')
param publicUrls {
  contentApi: string
  publicSite: string
  publicApi: string
}

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool = false

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Data Processor Function App.')
param dataProcessorAppRegistrationClientId string = ''

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the API Container App.')
param apiAppRegistrationClientId string = ''

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

//
// Define our resource prefix.
//

// The resource prefix for anything specific to the Public API. 
var publicApiResourcePrefix = '${subscription}-ees-papi'

// The resource prefix for anything not specific solely to the Public API but set up within Bicep. 
var commonResourcePrefix = '${subscription}-ees'

// The resource prefix for resources created in the ARM template.
var legacyResourcePrefix = subscription


//
// Define the full names of all resources.
//

// Define full names for the pre-existing resources managed by the ARM template.
var adminAppName = '${legacyResourcePrefix}-as-ees-admin'
var publisherAppFullName = '${legacyResourcePrefix}-fa-ees-publisher'
var coreStorageAccountName = '${legacyResourcePrefix}saeescore'
var keyVaultName = '${legacyResourcePrefix}-kv-ees-01'
var vNetName = '${legacyResourcePrefix}-vnet-ees'
var alertsGroupFullName = '${legacyResourcePrefix}-ag-ees-alertedusers'
var acrName = 'eesacr'

// Define full names for new resources managed by the Bicep deployment.
var dataProcessorAppName = '${publicApiResourcePrefix}-fa-processor'
var dataProcessorAppServicePlanName = '${publicApiResourcePrefix}-asp-processor'
var dataProcessorStorageAccountsPrefix = '${subscription}eessaprocessor'
var psqlServerName = '${subscription}-ees-psql-flexibleserver'
var containerAppEnvironmentName = '${commonResourcePrefix}-cae-01'
var apiAppName = '${publicApiResourcePrefix}-ca-api'
var appGatewayName = '${commonResourcePrefix}-agw-01'
var fileShareName = '${publicApiResourcePrefix}-fs-data'
var appInsightsName = '${publicApiResourcePrefix}-ai'
var logAnalyticsWorkspaceName = '${commonResourcePrefix}-log'
var publicApiStorageAccountName = '${subscription}eespapisa'
var dataFilesFileShareMountName = 'public-api-fileshare-mount'
var dataFilesFileShareMountPath = '/data/public-api-data'

// Define full names for managed identities used by Bicep-managed resources.
var apiAppIdentityName = '${publicApiResourcePrefix}-id-ca-api'
var dataProcessorIdentityName = '${publicApiResourcePrefix}-id-fa-processor'
var appGatewayIdentityName = '${commonResourcePrefix}-id-agw-01'

// Reference the existing Azure Container Registry resource as currently managed by the EES ARM template.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

// Reference the existing core Storage Account as currently managed by the EES ARM template.
resource coreStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: coreStorageAccountName
  scope: resourceGroup(resourceGroup().name)
}
var coreStorageAccountKey = coreStorageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var coreStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${coreStorageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${coreStorageAccountKey}'

// Reference the existing VNet as currently managed by the EES ARM template, and register new subnets for Bicep-controlled resources.
module vNetModule 'application/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    vNetName: vNetName
    publicApiResourcePrefix: publicApiResourcePrefix
    commonResourcePrefix: commonResourcePrefix
    legacyResourcePrefix: legacyResourcePrefix
    containerAppEnvironmentName: containerAppEnvironmentName
    dataProcessorName: dataProcessorAppName
    adminAppServiceName: adminAppName
    psqlFlexibleServerName: psqlServerName
    publisherFunctionAppName: publisherAppFullName
    appGatewayName: appGatewayName
  }
}

module privateDnsZonesModule 'application/privateDnsZones.bicep' = {
  name: 'privateDnsZonesDeploy'
  params: {
    vnetName: vNetName
  }
}

// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
// Currently supported by subnet whitelisting and Storage service endpoints being enabled on the whitelisted subnets.
module publicApiStorageAccountModule 'components/storageAccount.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: publicApiStorageAccountName
    allowedSubnetIds: [
      vNetModule.outputs.dataProcessorSubnetRef
      vNetModule.outputs.containerAppEnvironmentSubnetRef
    ]
    firewallRules: storageFirewallRules
    skuStorageResource: 'Standard_LRS'
    keyVaultName: keyVaultName
    tagValues: tagValues
  }
}

// TODO EES-5128 - we're currently needing to look up the Public API Storage Account in order to get its access keys,
// as it's not possible to feed Key Vault secret references into the "storageAccountKey" values for Azure File Storage
// mounts.
//
// It would be possible to use KV references if restructuring main.bicep to make the creation of the Container App
// and Data Processor their own sub-modules in the "application" folder.  Then, we could use @secure() params
// and keyVaultResource.getSecret() to pass the secrets through.
resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: publicApiStorageAccountName
}

var publicApiStorageAccountAccessKey = publicApiStorageAccount.listKeys().keys[0].value

// Deploy File Share.
module dataFilesFileShareModule 'components/fileShare.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: fileShareName
    fileShareQuota: fileShareQuota
    storageAccountName: publicApiStorageAccountName
    fileShareAccessTier: 'TransactionOptimized'
  }
  dependsOn: [
    publicApiStorageAccountModule
  ]
}

// Deploy a single shared Application Insights for all relevant Public API resources to use.
module applicationInsightsModule 'components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    location: location
    appInsightsName: appInsightsName
  }
}

// Create a generic, shared Log Analytics Workspace for any relevant resources to use.
module logAnalyticsWorkspaceModule 'components/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceDeploy'
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    location: location
    tagValues: tagValues
  }
}

var formattedPostgreSqlFirewallRules = map(postgreSqlFirewallRules, rule => {
  name: replace(rule.name, ' ', '_')
  cidr: rule.cidr
})

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = if (updatePsqlFlexibleServer) {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    databaseServerName: psqlServerName
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
    subnetId: vNetModule.outputs.psqlFlexibleServerSubnetRef
  }
  dependsOn: [
    privateDnsZonesModule
  ]
}

var psqlManagedIdentityConnectionStringTemplate = 'Server=${psqlServerName}.postgres.database.azure.com;Database=[database_name];Port=5432;User Id=[managed_identity_name]'

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: apiAppIdentityName
  location: location
}

module apiContainerAppAcrPullRoleAssignmentModule 'components/containerRegistryRoleAssignment.bicep' = {
  name: '${apiAppIdentityName}AcrPullRoleAssignmentDeploy'
  params: {
    role: 'AcrPull'
    containerRegistryName: acrName
    principalIds: [apiContainerAppManagedIdentity.properties.principalId]
  }
}

module containerAppEnvironmentModule 'application/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    location: location
    containerAppEnvironmentName: containerAppEnvironmentName
    subnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    publicApiStorageAccountName: publicApiStorageAccountName
    publicApiFileShareName: dataFilesFileShareModule.outputs.fileShareName
    publicApiStorageAccountAccessKey: publicApiStorageAccountAccessKey
    tagValues: tagValues
  }
}

// Deploy main Public API Container App.
module apiContainerAppModule 'components/containerApp.bicep' = if (deployContainerApp) {
  name: 'apiContainerAppDeploy'
  params: {
    location: location
    containerAppName: apiAppName
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: 'ees-public-api/api:${dockerImagesTag}'
    userAssignedManagedIdentityId: apiContainerAppManagedIdentity.id
    managedEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    corsPolicy: {
      allowedOrigins: [
        publicUrls.publicSite
        'http://localhost:3000'
        'http://127.0.0.1'
      ]
    }
    volumeMounts: [
      {
        volumeName: dataFilesFileShareMountName
        mountPath: dataFilesFileShareMountPath
      }
    ]
    volumes: [
      {
        name: dataFilesFileShareMountName
        storageType: 'AzureFile'
        storageName: dataFilesFileShareModule.outputs.fileShareName
      }
    ]
    appSettings: [
      // TODO EES-5128 - replace this with a Key Vault reference string.
      {
        name: 'ConnectionStrings__PublicDataDb'
        value: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', apiAppIdentityName)
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
        value: publicUrls.contentApi
      }
      {
        name: 'MiniProfiler__Enabled'
        value: 'true'
      }
      {
        name: 'DataFiles__BasePath'
        value: dataFilesFileShareMountPath
      }
      {
        name: 'OpenIdConnect__TenantId'
        value: tenant().tenantId
      }
      {
        name: 'OpenIdConnect__ClientId'
        value: apiAppRegistrationClientId
      }
    ]
    entraIdAuthentication: {
      appRegistrationClientId: apiAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
      ]
      allowedPrincipalIds: [
        adminAppPrincipalId
      ]
      requireAuthentication: false
    }
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
    apiContainerAppAcrPullRoleAssignmentModule
  ]
}

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: adminAppName
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId
var adminAppPrincipalId = adminAppService.identity.principalId

resource dataProcessorFunctionAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: dataProcessorIdentityName
  location: location
}

// Deploy Data Processor Function.
module dataProcessorFunctionAppModule 'components/functionApp.bicep' = {
  name: 'dataProcessorFunctionAppDeploy'
  params: {
    functionAppName: dataProcessorAppName
    appServicePlanName: dataProcessorAppServicePlanName
    storageAccountsNamePrefix: dataProcessorStorageAccountsPrefix
    alertsGroupName: alertsGroupFullName
    location: location
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
    privateEndpointSubnetId: vNetModule.outputs.dataProcessorPrivateEndpointSubnetRef
    publicNetworkAccessEnabled: false
    entraIdAuthentication: {
      appRegistrationClientId: dataProcessorAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
      ]
      allowedPrincipalIds: [
        adminAppPrincipalId
      ]
      requireAuthentication: true
    }
    userAssignedManagedIdentityParams: {
      id: dataProcessorFunctionAppManagedIdentity.id
      name: dataProcessorFunctionAppManagedIdentity.name
      principalId: dataProcessorFunctionAppManagedIdentity.properties.principalId
    }
    functionAppExists: dataProcessorFunctionAppExists
    keyVaultName: keyVaultName
    functionAppRuntime: 'dotnet-isolated'
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    preWarmedInstanceCount: 1
    healthCheck: {
      path: '/api/HealthCheck'
      unhealthyMetricName: '${subscription}PublicDataProcessorUnhealthy'
    }
    appSettings: {
      AppSettings__MetaInsertBatchSize: 1000
    }
    azureFileShares: [{
      storageName: dataFilesFileShareModule.outputs.fileShareName
      storageAccountKey: publicApiStorageAccountAccessKey
      storageAccountName: publicApiStorageAccountName
      fileShareName: dataFilesFileShareModule.outputs.fileShareName
      mountPath: dataFilesFileShareMountPath
    }]
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
  ]
}

// Create an Application Gateway to serve public traffic for the Public API Container App.
module appGatewayModule 'components/appGateway.bicep' = {
  name: 'appGatewayDeploy'
  params: {
    location: location
    appGatewayName: appGatewayName
    managedIdentityName: appGatewayIdentityName
    keyVaultName: keyVaultName
    subnetId: vNetModule.outputs.appGatewaySubnetRef
    sites: [
      {
        resourceName: apiContainerAppModule.outputs.containerAppName
        backendFqdn: apiContainerAppModule.outputs.containerAppFqdn
        publicFqdn: replace(publicUrls.publicApi, 'https://', '')
        certificateKeyVaultSecretName: '${apiContainerAppModule.outputs.containerAppName}-certificate'
        healthProbeRelativeUrl: '/docs'
      }
    ]
    tagValues: tagValues
  }
}

var dataProcessorPsqlConnectionStringSecretKey = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

module storeDataProcessorPsqlConnectionString 'components/keyVaultSecret.bicep' = {
  name: 'storeDataProcessorPsqlConnectionString'
  params: {
    keyVaultName: keyVaultName
    isEnabled: true
    secretName: dataProcessorPsqlConnectionStringSecretKey
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', dataProcessorFunctionAppManagedIdentity.name)
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
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', publisherAppFullName)
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
    secretValue: replace(replace(psqlManagedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', adminAppName)
    contentType: 'text/plain'
  }
}

var coreStorageConnectionStringSecretKey = 'ees-core-storage-connectionstring'

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

output dataProcessorContentDbConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-contentdb'
output dataProcessorPsqlConnectionStringSecretKey string = dataProcessorPsqlConnectionStringSecretKey
output dataProcessorFunctionAppManagedIdentityClientId string = dataProcessorFunctionAppManagedIdentity.properties.clientId

output coreStorageConnectionStringSecretKey string = coreStorageConnectionStringSecretKey
output keyVaultName string = keyVaultName

output dataFilesFileShareMountPath string = dataFilesFileShareMountPath
