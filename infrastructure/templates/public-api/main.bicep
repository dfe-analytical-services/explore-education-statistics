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
// Define full names for the pre-existing resources managed by the ARM template.
//
var adminAppName = '${legacyResourcePrefix}-as-ees-admin'
var publisherAppFullName = '${legacyResourcePrefix}-fa-ees-publisher'
var keyVaultName = '${legacyResourcePrefix}-kv-ees-01'
var vNetName = '${legacyResourcePrefix}-vnet-ees'
var alertsGroupName = '${legacyResourcePrefix}-ag-ees-alertedusers'
var acrName = 'eesacr'

module vNetModule 'application/shared/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    vNetName: vNetName
    publicApiResourcePrefix: publicApiResourcePrefix
    commonResourcePrefix: commonResourcePrefix
    legacyResourcePrefix: legacyResourcePrefix
  }
}

module coreStorage 'application/shared/coreStorage.bicep' = {
  name: 'coreStorageModuleDeploy'
  params: {
    legacyResourcePrefix: legacyResourcePrefix
    keyVaultName: keyVaultName
  }
}

module privateDnsZonesModule 'application/shared/privateDnsZones.bicep' = {
  name: 'privateDnsZonesDeploy'
  params: {
    vnetName: vNetName
  }
}

module publicApiStorageModule 'application/public-api/publicApiStorage.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    publicApiResourcePrefix: publicApiResourcePrefix
    subscription: subscription
    keyVaultName: keyVaultName
    dataProcessorSubnetId: vNetModule.outputs.dataProcessorSubnetRef
    containerAppEnvironmentSubnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    publicApiDataFileShareQuota: publicApiDataFileShareQuota
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

module appInsightsModule 'application/public-api/publicApiAppInsights.bicep' = {
  name: 'publicApiAppInsightsModuleDeploy'
  params: {
    location: location
    publicApiResourcePrefix: publicApiResourcePrefix
  }
}

// Create a generic, shared Log Analytics Workspace for any relevant resources to use.
module logAnalyticsWorkspaceModule 'application/shared/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceModuleDeploy'
  params: {
    location: location
    commonResourcePrefix: commonResourcePrefix
    tagValues: tagValues
  }
}

module postgreSqlServerModule 'application/shared/postgreSqlFlexibleServer.bicep' = if (updatePsqlFlexibleServer) {
  name: 'postgreSqlFlexibleServerModuleDeploy'
  params: {
    location: location
    commonResourcePrefix: commonResourcePrefix
    postgreSqlAdminName: postgreSqlAdminName
    postgreSqlAdminPassword: postgreSqlAdminPassword!
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

module containerAppEnvironmentModule 'application/shared/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentModuleDeploy'
  params: {
    location: location
    commonResourcePrefix: commonResourcePrefix
    subnetId: vNetModule.outputs.containerAppEnvironmentSubnetRef
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceName
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    publicApiStorageAccountName: publicApiStorageModule.outputs.storageAccountName
    publicApiFileShareName: publicApiStorageModule.outputs.publicApiDataFileShareName
    tagValues: tagValues
  }
}

// Deploy main Public API Container App.
module apiAppModule 'application/public-api/publicApiApp.bicep' = if (deployContainerApp) {
  name: 'publicApiAppModuleDeploy'
  params: {
    location: location
    publicApiResourcePrefix: publicApiResourcePrefix
    acrName: acrName
    adminAppName: adminAppName
    apiAppRegistrationClientId: apiAppRegistrationClientId
    containerAppEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    contentApiUrl: publicUrls.contentApi
    publicApiUrl: publicUrls.publicApi
    dockerImagesTag: dockerImagesTag
    publicApiDbConnectionStringTemplate: postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
    publicApiDataFileShareName: publicApiStorageModule.outputs.publicApiDataFileShareName
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
  ]
}

module dataProcessorModule 'application/public-api/publicApiDataProcessor.bicep' = {
  name: 'publicApiDataProcessorModuleDeploy'
  params: {
    location: location
    subscription: subscription
    publicApiResourcePrefix: publicApiResourcePrefix
    adminAppName: adminAppName
    keyVaultName: keyVaultName
    metricsNamePrefix: '${subscription}PublicDataProcessor'
    alertsGroupName: alertsGroupName
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
    privateEndpointSubnetId: vNetModule.outputs.dataProcessorPrivateEndpointSubnetRef
    dataProcessorAppRegistrationClientId: dataProcessorAppRegistrationClientId
    publicApiDataFileShareName: publicApiStorageModule.outputs.publicApiDataFileShareName
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
module appGatewayModule 'application/shared/appGateway.bicep' = {
  name: 'appGatewayModuleDeploy'
  params: {
    location: location
    commonResourcePrefix: commonResourcePrefix
    keyVaultName: keyVaultName
    subnetId: vNetModule.outputs.appGatewaySubnetRef
    publicApiContainerAppSettings: {
      name: apiAppModule.outputs.containerAppName
      backendFqdn: apiAppModule.outputs.containerAppFqdn
      publicFqdn: replace(publicUrls.publicApi, 'https://', '')
      certificateName: '${apiAppModule.outputs.containerAppName}-certificate'
      healthProbeRelativeUrl: apiAppModule.outputs.containerAppHealthProbeRelativeUrl
    }
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
    secretValue: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', dataProcessorModule.outputs.managedIdentityName)
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

output dataProcessorContentDbConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-contentdb'
output dataProcessorPsqlConnectionStringSecretKey string = dataProcessorPsqlConnectionStringSecretKey
output dataProcessorFunctionAppManagedIdentityClientId string = dataProcessorModule.outputs.managedIdentityClientId

output coreStorageConnectionStringSecretKey string = coreStorage.outputs.coreStorageConnectionStringSecretKey
output keyVaultName string = keyVaultName

output dataProcessorPublicApiDataFileShareMountPath string = dataProcessorModule.outputs.publicApiDataFileShareMountPath
