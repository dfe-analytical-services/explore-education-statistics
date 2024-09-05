@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int = 1

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
var alertsGroupName = '${legacyResourcePrefix}-ag-ees-alertedusers'
var acrName = 'eesacr'

// Define full names for new resources managed by the Bicep deployment.
var dataProcessorAppName = '${publicApiResourcePrefix}-fa-processor'
var dataProcessorAppServicePlanName = '${publicApiResourcePrefix}-asp-processor'
var dataProcessorStorageAccountsPrefix = '${subscription}eessaprocessor'
var postgreSqlServerName = '${subscription}-ees-psql-flexibleserver'
var containerAppEnvironmentName = '${commonResourcePrefix}-cae-01'
var apiAppName = '${publicApiResourcePrefix}-ca-api'
var appGatewayName = '${commonResourcePrefix}-agw-01'
var publicApiDataFileShareName = '${publicApiResourcePrefix}-fs-data'
var appInsightsName = '${publicApiResourcePrefix}-ai'
var logAnalyticsWorkspaceName = '${commonResourcePrefix}-log'
var publicApiStorageAccountName = '${subscription}eespapisa'
var dataFilesFileShareMountPath = '/data/public-api-data'

// Define full names for managed identities used by Bicep-managed resources.
var apiAppIdentityName = '${publicApiResourcePrefix}-id-ca-api'
var dataProcessorIdentityName = '${publicApiResourcePrefix}-id-fa-processor'
var appGatewayIdentityName = '${commonResourcePrefix}-id-agw-01'

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
    psqlFlexibleServerName: postgreSqlServerName
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

module publicApiStorageModule 'application/publicApiStorage.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    keyVaultName: keyVaultName
    dataProcessorSubnetId: vNetModule.outputs.dataProcessorSubnetRef
    containerAppEnvironmentSubnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    publicApiStorageAccountName: publicApiStorageAccountName
    publicApiDataFileShareName: publicApiDataFileShareName
    publicApiDataFileShareQuota: publicApiDataFileShareQuota
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
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

module postgreSqlServerModule 'application/postgreSqlFlexibleServer.bicep' = if (updatePsqlFlexibleServer) {
  name: 'postgreSqlFlexibleServerDeploy'
  params: {
    location: location
    postgreSqlServerName: postgreSqlServerName
    postgreSqlAdminName: postgreSqlAdminName
    postgreSqlAdminPassword: postgreSqlAdminPassword
    privateEndpointSubnetId: vNetModule.outputs.psqlFlexibleServerSubnetRef
    postgreSqlAutoGrowStatus: postgreSqlAutoGrowStatus
    postgreSqlFirewallRules: postgreSqlFirewallRules
    postgreSqlSkuName: postgreSqlSkuName
    postgreSqlStorageSizeGB: postgreSqlStorageSizeGB
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
  ]
}

module containerAppEnvironmentModule 'application/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentDeploy'
  params: {
    location: location
    containerAppEnvironmentName: containerAppEnvironmentName
    subnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    publicApiStorageAccountName: publicApiStorageModule.outputs.storageAccountName
    publicApiFileShareName: publicApiDataFileShareName
    tagValues: tagValues
  }
}

// Deploy main Public API Container App.
module apiAppModule 'application/apiApp.bicep' = if (deployContainerApp) {
  name: 'apiAppDeploy'
  params: {
    location: location
    acrName: acrName
    adminAppName: adminAppName
    apiAppIdentityName: apiAppIdentityName
    apiAppName: apiAppName
    apiAppRegistrationClientId: apiAppRegistrationClientId
    containerAppEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    contentApiUrl: publicUrls.contentApi
    publicApiUrl: publicUrls.publicApi
    dockerImagesTag: dockerImagesTag
    publicApiDbConnectionStringTemplate: postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
    publicApiDataFileShareName: publicApiDataFileShareName
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
  ]
}

module dataProcessorModule 'application/dataProcessor.bicep' = {
  name: 'dataProcessorDeploy'
  params: {
    location: location
    dataProcessorAppName: dataProcessorAppName
    appServicePlanName: dataProcessorAppServicePlanName
    dataProcessorIdentityName: dataProcessorIdentityName
    adminAppName: adminAppName
    keyVaultName: keyVaultName
    metricsNamePrefix: '${subscription}PublicDataProcessor'
    alertsGroupName: alertsGroupName
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
    privateEndpointSubnetId: vNetModule.outputs.dataProcessorPrivateEndpointSubnetRef
    dataProcessorAppRegistrationClientId: dataProcessorAppRegistrationClientId
    dataProcessorStorageAccountsPrefix: dataProcessorStorageAccountsPrefix
    publicApiDataFileShareName: publicApiDataFileShareName
    publicApiStorageAccountName: publicApiStorageModule.outputs.storageAccountName
    storageFirewallRules: storageFirewallRules
    dataProcessorFunctionAppExists: dataProcessorFunctionAppExists
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
        resourceName: apiAppModule.outputs.containerAppName
        backendFqdn: apiAppModule.outputs.containerAppFqdn
        publicFqdn: replace(publicUrls.publicApi, 'https://', '')
        certificateKeyVaultSecretName: '${apiAppModule.outputs.containerAppName}-certificate'
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
    secretValue: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', dataProcessorIdentityName)
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
    secretValue: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', publisherAppFullName)
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
    secretValue: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', adminAppName)
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
output dataProcessorFunctionAppManagedIdentityClientId string = dataProcessorModule.outputs.managedIdentityClientId

output coreStorageConnectionStringSecretKey string = coreStorageConnectionStringSecretKey
output keyVaultName string = keyVaultName

output dataFilesFileShareMountPath string = dataFilesFileShareMountPath
