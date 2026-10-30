import { ServiceHttpStatusAlerts } from '../common/components/alerts/httpStatusAlertConfig.bicep'
import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'
import { StorageAccountSku } from '../common/components/storage/types.bicep'
import { Tags } from 'types.bicep'
import { EnvironmentConfig, EnvironmentPipelineVariables, mergeEnvironmentConfig } from 'configuration/environment-configuration.bicep'
import { AdminConfig, mergeAdminConfig } from 'configuration/admin-configuration.bicep'
import { ContentApiConfig, mergeContentApiConfig } from 'configuration/content-api-configuration.bicep'
import { DataApiConfig, mergeDataApiConfig } from 'configuration/data-api-configuration.bicep'
import { ImporterConfig, mergeImporterConfig } from 'configuration/importer-configuration.bicep'
import { NotifierConfig, mergeNotifierConfig } from 'configuration/notifier-configuration.bicep'
import { PublisherConfig, mergePublisherConfig } from 'configuration/publisher-configuration.bicep'
import { PublicSiteConfig, mergePublicSiteConfig } from 'configuration/public-site-configuration.bicep'

@description('Per-service overrides. Empty preserves all existing Http401 rules.')
param http401Alerts ServiceHttpStatusAlerts = {}

@description('Per-service overrides. Empty preserves all existing Http5xx rules.')
param http5xxAlerts ServiceHttpStatusAlerts = {}

//
// Tagging config.
//

@description('Tags for tagging resources created in Azure. These are all fed in from pipeline variables.')
param tags Tags = {
  CostCentre: ''
  Department: ''
  Solution: ''
  ServiceOwner: ''
  CreatedBy: ''
  DeploymentRepoUrl: ''
}

@description('Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')



//
// Environment-wide config.
//
param environmentConfigParam EnvironmentConfig = {}

// Merge default configuration with overridden configuration.
var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)

// These values are all supplied specifically by pipeline variables.
param environmentPipelineVariables EnvironmentPipelineVariables = {}

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

@description('The operating system to host every App Service and Function App on. An App Service plan can\'t change operating system in place, so only the param files in "parameters/elz" set Linux.')
param appOperatingSystem 'Windows' | 'Linux' = 'Windows'

@description('Whether the Admin, Content API, Data API and Public Site App Service plans spread their instances across availability zones. Needs Premium SKUs, and runs at least 2 instances. Only the Pre-Production and Production param files in "parameters/elz" turn this on.')
param zoneRedundantAppServicePlans bool = false

@description('SKU of the Importer Function App\'s storage account.')
param importerStorageAccountSku StorageAccountSku = 'Standard_LRS'

@description('SKU of the Notifier Function App\'s storage account, which holds the email subscriptions.')
param notifierStorageAccountSku StorageAccountSku = 'Standard_LRS'

@description('SKU of the Publisher Function App\'s storage account, which holds the release publishing statuses.')
param publisherStorageAccountSku StorageAccountSku = 'Standard_GZRS'

@description('Whether to deploy a user-assigned managed identity for each of this pipeline\'s apps, for them to use instead of system-assigned ones. Only the param files in "parameters/elz" turn this on.')
param deployAppIdentities bool = false

// Pipeline-supplied tags, plus those that vary by environment and deployment.
var tagValues = union(tags, {
  Environment: environmentConfig.environmentName!
  DateProvisioned: dateProvisioned
})



//
// Admin-specific config.
//
param adminConfigParam AdminConfig = {}

// Merge default configuration with overridden configuration from params files.
var adminConfig = mergeAdminConfig(adminConfigParam)

@secure()
@description('The existing appsettings for the Admin App Service production slot, fetched by the pipeline before deployment.')
param adminProdAppSettings object = {}

@secure()
@description('The existing appsettings for the Admin App Service staging slot, fetched by the pipeline before deployment.')
param adminStagingSlotAppSettings object = {}



//
// Content API-specific config.
//
param contentApiConfigParam ContentApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var contentApiConfig = mergeContentApiConfig(contentApiConfigParam)

@secure()
@description('The existing appsettings for the Content API App Service production slot, fetched by the pipeline before deployment.')
param contentApiProdAppSettings object = {}

@secure()
@description('The existing appsettings for the Content API App Service staging slot, fetched by the pipeline before deployment.')
param contentApiStagingSlotAppSettings object = {}



//
// Data API-specific config.
//
param dataApiConfigParam DataApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var dataApiConfig = mergeDataApiConfig(dataApiConfigParam)

@secure()
@description('The existing appsettings for the Data API App Service production slot, fetched by the pipeline before deployment.')
param dataApiProdAppSettings object = {}

@secure()
@description('The existing appsettings for the Data API App Service staging slot, fetched by the pipeline before deployment.')
param dataApiStagingSlotAppSettings object = {}



//
// Importer-specific config.
//
param importerConfigParam ImporterConfig = {}

// Merge default configuration with overridden configuration from params files.
var importerConfig = mergeImporterConfig(importerConfigParam)

@secure()
@description('The existing appsettings for the Importer Function App, fetched by the pipeline before deployment.')
param importerProdAppSettings object = {}



//
// Notifier-specific config.
//
param notifierConfigParam NotifierConfig = {}

// Merge default configuration with overridden configuration from params files.
var notifierConfig = mergeNotifierConfig(notifierConfigParam)

@secure()
@description('The existing appsettings for the Notifier Function App, fetched by the pipeline before deployment.')
param notifierProdAppSettings object = {}



//
// Publisher-specific config.
//
param publisherConfigParam PublisherConfig = {}

// Merge default configuration with overridden configuration from params files.
var publisherConfig = mergePublisherConfig(publisherConfigParam)

@secure()
@description('The existing appsettings for the Publisher Function App, fetched by the pipeline before deployment.')
param publisherProdAppSettings object = {}



//
// Public site-specific config.
//
param publicSiteConfigParam PublicSiteConfig = {}

// Merge default configuration with overridden configuration from params files.
var publicSiteConfig = mergePublicSiteConfig(publicSiteConfigParam)

@secure()
@description('The existing appsettings for the Public Site App Service, fetched by the pipeline before deployment.')
param publicSiteProdAppSettings object = {}



//
// Resource provisioning.
//

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: environmentConfig.environmentIdentifier!
  environmentName: environmentConfig.environmentName!
  elz: elz
})

var minTlsVersion = '1.2'

var logAnalyticsWorkspaceId = resourceId('Microsoft.OperationalInsights/workspaces', resourceNames.logAnalyticsWorkspace)

var afdEndpointResourceId = resourceId('Microsoft.Cdn/profiles/afdEndpoints', resourceNames.frontDoor.frontDoorName, resourceNames.frontDoor.defaultEndpoint.endpointName)

var basePublicAllowedOrigins = [
  'https://${environmentConfig.?domain}'
  'https://${resourceNames.publicSite.appService}.azurewebsites.net'
  'https://${reference(afdEndpointResourceId, '2025-06-01').hostName}'
]

var publicSiteAllowedOrigins = union(basePublicAllowedOrigins, environmentConfig.?additionalPublicAllowedOrigins ?? [])

var baseAdminAllowedOrigins = [
  'https://admin.${environmentConfig.domain!}'
  'https://${resourceNames.admin.appService}.azurewebsites.net'
]

var adminSiteAllowedOrigins = union(baseAdminAllowedOrigins, environmentConfig.?additionalAdminAllowedOrigins ?? [])

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

// Used to encrypt the ASP.NET Core Data Protection key ring for Windows App Services, as the key ring
// can't safely live on local disk when shared across deployment slots.
resource dataProtectionKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: keyVault
  name: resourceNames.keyVault.keys.dataProtection
  properties: {
    kty: 'RSA'
    keySize: 2048
    keyOps: [
      'wrapKey'
      'unwrapKey'
    ]
  }
}

module appIdentitiesModule '../common/components/managed-identity/user-assigned-identities.bicep' = if (deployAppIdentities) {
  name: 'appIdentitiesModuleDeploy'
  params: {
    identityNames: [
      resourceNames.admin.identity
      resourceNames.contentApi.identity
      resourceNames.dataApi.identity
      resourceNames.publicSite.identity
      resourceNames.importer.identity
      resourceNames.notifier.identity
      resourceNames.publisher.identity
    ]
    tagValues: tagValues
  }
}

module importerModuleDeploy '../importer/main.bicep' = {
  name: 'importerModuleDeploy'
  params: {
    useUserAssignedIdentity: deployAppIdentities
    resourceNames: resourceNames
    operatingSystem: appOperatingSystem
    storageAccountSku: importerStorageAccountSku
    appServiceSku: importerConfig.appServiceSku!
    deployAlerts: true
    http401Alert: http401Alerts.?importer
    http5xxAlert: http5xxAlerts.?importer
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.importer.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    existingAppSettings: importerProdAppSettings
    tagValues: tagValues
  }
  dependsOn: [
    appIdentitiesModule
  ]
}

module notifierModuleDeploy '../notifier/main.bicep' = {
  name: 'notifierModuleDeploy'
  params: {
    useUserAssignedIdentity: deployAppIdentities
    resourceNames: resourceNames
    operatingSystem: appOperatingSystem
    storageAccountSku: notifierStorageAccountSku
    appServiceSku: notifierConfig.appServiceSku!
    allowedOrigins: publicSiteAllowedOrigins
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.notifier.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    blobDeleteRetentionDays: environmentConfig.blobDeleteRetentionDays!
    existingAppSettings: notifierProdAppSettings
    deployAlerts: true
    http401Alert: http401Alerts.?notifier
    http5xxAlert: http5xxAlerts.?notifier
    tagValues: tagValues
  }
  dependsOn: [
    appIdentitiesModule
  ]
}

module publisherModuleDeploy '../publisher/main.bicep' = {
  name: 'publisherModuleDeploy'
  params: {
    useUserAssignedIdentity: deployAppIdentities
    resourceNames: resourceNames
    operatingSystem: appOperatingSystem
    storageAccountSku: publisherStorageAccountSku
    appServiceSku: publisherConfig.appServiceSku!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.publisher.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    blobDeleteRetentionDays: environmentConfig.blobDeleteRetentionDays!
    existingAppSettings: publisherProdAppSettings
    deployAlerts: true
    http401Alert: http401Alerts.?publisher
    http5xxAlert: http5xxAlerts.?publisher
    tagValues: tagValues
  }
  dependsOn: [
    appIdentitiesModule
    // Publisher is dependent on Notifier's storage account being available
    // in order to reference its connection string secret in Key Vault.
    notifierModuleDeploy
  ]
}

module adminModuleDeploy '../admin/main.bicep' = {
  name: 'adminModuleDeploy'
  params: {
    resourceNames: resourceNames
    zoneRedundant: zoneRedundantAppServicePlans
    operatingSystem: appOperatingSystem
    appServiceSku: adminConfig.appServiceSku!
    signalRAllowedOrigins: adminSiteAllowedOrigins
    signalRSku: adminConfig.signalRSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    deployAlerts: true
    http401Alert: http401Alerts.?admin
    http5xxAlert: http5xxAlerts.?admin
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.admin.databaseUserPassword)
    existingProdAppSettings: adminProdAppSettings
    existingStagingSlotAppSettings: adminStagingSlotAppSettings
    tagValues: tagValues
  }
  dependsOn: [
    // Admin is dependent on Importer's storage account being available
    // in order to reference its connection string secret in Key Vault.
    importerModuleDeploy
  ]
}

module contentApiModuleDeploy '../content-api/main.bicep' = {
  name: 'contentApiModuleDeploy'
  params: {
    resourceNames: resourceNames
    zoneRedundant: zoneRedundantAppServicePlans
    operatingSystem: appOperatingSystem
    appServiceSku: contentApiConfig.appServiceSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    deployAlerts: true
    http401Alert: http401Alerts.?contentApi
    http5xxAlert: http5xxAlerts.?contentApi
    detailedErrors: environmentConfig.detailedErrors!
    restrictOriginToFrontDoor: contentApiConfig.restrictOriginToFrontDoor!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.contentApi.databaseUserPassword)
    existingProdAppSettings: contentApiProdAppSettings
    existingStagingSlotAppSettings: contentApiStagingSlotAppSettings
    tagValues: tagValues
  }
}

module dataApiModuleDeploy '../data-api/main.bicep' = {
  name: 'dataApiModuleDeploy'
  params: {
    resourceNames: resourceNames
    zoneRedundant: zoneRedundantAppServicePlans
    operatingSystem: appOperatingSystem
    appServiceSku: dataApiConfig.appServiceSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    deployAlerts: true
    http401Alert: http401Alerts.?dataApi
    http5xxAlert: http5xxAlerts.?dataApi
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.dataApi.databaseUserPassword)
    existingProdAppSettings: dataApiProdAppSettings
    existingStagingSlotAppSettings: dataApiStagingSlotAppSettings
    tagValues: tagValues
  }
}

module publicSiteModuleDeploy '../public-site/main.bicep' = {
  name: 'publicSiteModuleDeploy'
  params: {
    resourceNames: resourceNames
    zoneRedundant: zoneRedundantAppServicePlans
    appServiceSku: publicSiteConfig.appServiceSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    deployAlerts: true
    http401Alert: http401Alerts.?publicSite
    http5xxAlert: http5xxAlerts.?publicSite
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    existingProdAppSettings: publicSiteProdAppSettings
    tagValues: tagValues
  }
}
