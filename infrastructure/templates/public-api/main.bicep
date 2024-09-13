import { abbreviations } from 'abbreviations.bicep'
import { firewallRuleType } from 'types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int = 1

@description('Public API Storage : Firewall rules.')
param storageFirewallRules firewallRuleType[] = []

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
param postgreSqlFirewallRules firewallRuleType[] = []

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

var resourceNames = {
  existingResources: {
    adminApp: '${legacyResourcePrefix}-as-ees-admin'
    publisherFunction: '${legacyResourcePrefix}-fa-ees-publisher'
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    vNet: '${legacyResourcePrefix}-vnet-ees'
    alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
    acr: 'eesacr'
    // The Test Resource Group has broken from the naming convention of other environments for Core Storage
    coreStorageAccount: subscription == 's101t01' ? '${legacyResourcePrefix}storageeescore' : '${legacyResourcePrefix}saeescore'
    subnets: {
      adminApp: '${legacyResourcePrefix}-snet-ees-admin'
      publisherFunction: '${legacyResourcePrefix}-snet-ees-publisher'
      appGateway: '${commonResourcePrefix}-snet-${abbreviations.networkApplicationGateways}-01'
      containerAppEnvironment: '${commonResourcePrefix}-snet-${abbreviations.appManagedEnvironments}-01'
      dataProcessor: '${publicApiResourcePrefix}-snet-${abbreviations.webSitesFunctions}-processor'
      dataProcessorPrivateEndpoints: '${publicApiResourcePrefix}-snet-${abbreviations.webSitesFunctions}-processor-pep'
      psqlFlexibleServer: '${commonResourcePrefix}-snet-${abbreviations.dBforPostgreSQLServers}'
    }
  }
  sharedResources: {
    appGateway: '${commonResourcePrefix}-${abbreviations.networkApplicationGateways}-01'
    appGatewayIdentity: '${commonResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.networkApplicationGateways}-01'
    containerAppEnvironment: '${commonResourcePrefix}-${abbreviations.appManagedEnvironments}-01'
    logAnalyticsWorkspace: '${commonResourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
    postgreSqlFlexibleServer: '${commonResourcePrefix}-${abbreviations.dBforPostgreSQLServers}'
  }
  publicApi: {
    apiApp: '${publicApiResourcePrefix}-${abbreviations.appManagedEnvironments}-api'
    apiAppIdentity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.appManagedEnvironments}-api'
    appInsights: '${publicApiResourcePrefix}-${abbreviations.insightsComponents}'
    dataProcessor: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
    dataProcessorIdentity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.webSitesFunctions}-processor'
    // TODO - would this better be '${publicApiResourcePrefix}-${abbreviations.webServerFarms}-${abbreviations.webSitesFunctions}-processor' 
    dataProcessorPlan: '${publicApiResourcePrefix}-${abbreviations.webServerFarms}-processor'
    dataProcessorStorageAccountsPrefix: '${subscription}eessaprocessor'
    publicApiFileshare: '${publicApiResourcePrefix}-fs-data'
    publicApiStorageAccount: '${replace(publicApiResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}'
  }
}

module vNetModule 'application/shared/virtualNetwork.bicep' = {
  name: 'virtualNetworkApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module coreStorage 'application/shared/coreStorage.bicep' = {
  name: 'coreStorageApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module privateDnsZonesModule 'application/shared/privateDnsZones.bicep' = {
  name: 'privateDnsZonesApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module publicApiStorageModule 'application/public-api/publicApiStorage.bicep' = {
  name: 'publicApiStorageAccountApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    publicApiDataFileShareQuota: publicApiDataFileShareQuota
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

module appInsightsModule 'application/public-api/publicApiAppInsights.bicep' = {
  name: 'publicApiAppInsightsApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
  }
}

// Create a generic, shared Log Analytics Workspace for any relevant resources to use.
module logAnalyticsWorkspaceModule 'application/shared/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module postgreSqlServerModule 'application/shared/postgreSqlFlexibleServer.bicep' = if (updatePsqlFlexibleServer) {
  name: 'postgreSqlFlexibleServerApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    adminName: postgreSqlAdminName
    adminPassword: postgreSqlAdminPassword!
    privateEndpointSubnetId: vNetModule.outputs.psqlFlexibleServerSubnetRef
    autoGrowStatus: postgreSqlAutoGrowStatus
    firewallRules: postgreSqlFirewallRules
    sku: postgreSqlSkuName
    storageSizeGB: postgreSqlStorageSizeGB
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
  ]
}

module containerAppEnvironmentModule 'application/shared/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    tagValues: tagValues
  }
  dependsOn: [
    publicApiStorageModule
  ]
}

// Deploy main Public API Container App.
module apiAppModule 'application/public-api/publicApiApp.bicep' = if (deployContainerApp) {
  name: 'publicApiAppApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    apiAppRegistrationClientId: apiAppRegistrationClientId
    containerAppEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    contentApiUrl: publicUrls.contentApi
    publicApiUrl: publicUrls.publicApi
    dockerImagesTag: dockerImagesTag
    publicApiDbConnectionStringTemplate: postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
  ]
}

// Create an Application Gateway to serve public traffic for the Public API Container App.
module appGatewayModule 'application/shared/appGateway.bicep' = if (deployContainerApp) {
  name: 'appGatewayApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    publicApiContainerAppSettings: {
      resourceName: apiAppModule.outputs.containerAppName
      backendFqdn: apiAppModule.outputs.containerAppFqdn
      publicFqdn: replace(publicUrls.publicApi, 'https://', '')
      certificateName: '${apiAppModule.outputs.containerAppName}-certificate'
      healthProbeRelativeUrl: apiAppModule.outputs.containerAppHealthProbeRelativeUrl
    }
    tagValues: tagValues
  }
}

module dataProcessorModule 'application/public-api/publicApiDataProcessor.bicep' = {
  name: 'publicApiDataProcessorApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    metricsNamePrefix: '${subscription}PublicDataProcessor'
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    dataProcessorAppRegistrationClientId: dataProcessorAppRegistrationClientId
    storageFirewallRules: storageFirewallRules
    dataProcessorFunctionAppExists: dataProcessorFunctionAppExists
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
    publicApiStorageModule
  ]
}

output dataProcessorContentDbConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-contentdb'
output dataProcessorPsqlConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-publicdatadb'
output dataProcessorFunctionAppManagedIdentityClientId string = dataProcessorModule.outputs.managedIdentityClientId

output coreStorageConnectionStringSecretKey string = coreStorage.outputs.coreStorageConnectionStringSecretKey
output keyVaultName string = resourceNames.existingResources.keyVault

output dataProcessorPublicApiDataFileShareMountPath string = dataProcessorModule.outputs.publicApiDataFileShareMountPath
