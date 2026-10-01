import { getResourceNamesForEnvironment } from 'resource-names.bicep'
import { EnvironmentConfig, EnvironmentPipelineVariables, mergeEnvironmentConfig } from 'configuration/environment-configuration.bicep'
import { AdminConfig, mergeAdminConfig } from 'configuration/admin-configuration.bicep'
import { ContentApiConfig, mergeContentApiConfig } from 'configuration/content-api-configuration.bicep'
import { DataApiConfig, mergeDataApiConfig } from 'configuration/data-api-configuration.bicep'
import { ImporterConfig, mergeImporterConfig } from 'configuration/importer-configuration.bicep'
import { NotifierConfig, mergeNotifierConfig } from 'configuration/notifier-configuration.bicep'
import { PublisherConfig, mergePublisherConfig } from 'configuration/publisher-configuration.bicep'
import { PublicApiConfig, mergePublicApiConfig } from 'configuration/public-api-configuration.bicep'
import { PublicSiteConfig, mergePublicSiteConfig } from 'configuration/public-site-configuration.bicep'

//
// Tagging config.
//

@description('Tags for tagging resources created in Azure. These are all fed in from pipeline variables.')
param tags object



//
// Environment-wide config.
//
param environmentConfigParam EnvironmentConfig = {}

// Merge default configuration with overridden configuration.
var environmentConfig = mergeEnvironmentConfig(environmentConfigParam)

// These values are all supplied specifically by pipeline variables.
param environmentPipelineVariables EnvironmentPipelineVariables = {}



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



//
// Notifier-specific config.
//
param notifierConfigParam NotifierConfig = {}

// Merge default configuration with overridden configuration from params files.
var notifierConfig = mergeNotifierConfig(notifierConfigParam)



//
// Publisher-specific config.
//
param publisherConfigParam PublisherConfig = {}

// Merge default configuration with overridden configuration from params files.
var publisherConfig = mergePublisherConfig(publisherConfigParam)



//
// Public API-specific config.
//
param publicApiConfigParam PublicApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var publicApiConfig = mergePublicApiConfig(publicApiConfigParam)



//
// Public site-specific config.
//
param publicSiteConfigParam PublicSiteConfig = {}

// Merge default configuration with overridden configuration from params files.
var publicSiteConfig = mergePublicSiteConfig(publicSiteConfigParam)



//
// Resource provisioning.
//

var resourceNames = getResourceNamesForEnvironment(environmentConfig)

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

// TODO EES-7502 - use standardised hostname for the Content API .
var contentApiPublicHostname = '${environmentConfig.environmentName! == 'Pre-Production' ? 'cont' : 'content'}.${environmentConfig.domain!}'

var dataApiPublicHostname = 'data.${environmentConfig.domain!}'

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

var dockerRegistryUrl = 'https://${resourceNames.acr.serverName}${environment().suffixes.acrLoginServer}'

module importerModuleDeploy '../importer/main.bicep' = {
  name: 'importerModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: importerConfig.appServiceSku!
    deployAlerts: true
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.importer.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    tagValues: tags
  }
}

module notifierModuleDeploy '../notifier/main.bicep' = {
  name: 'notifierModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: notifierConfig.appServiceSku!
    suppressExceptionsForTeamOnlyApiKeyErrors: notifierConfig.suppressExceptionsForTeamOnlyApiKeyErrors!
    publicAppUrl: 'https://${environmentConfig.domain!}'
    allowedOrigins: publicSiteAllowedOrigins
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.notifier.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    blobDeleteRetentionDays: environmentConfig.blobDeleteRetentionDays!
    deployAlerts: true
    tagValues: tags
  }
}

module publisherModuleDeploy '../publisher/main.bicep' = {
  name: 'publisherModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: publisherConfig.appServiceSku!
    prepareScheduledReleaseVersionsNowEnabled: publisherConfig.prepareScheduledReleaseVersionsNowEnabled!
    publishScheduledReleaseVersionsNowEnabled: publisherConfig.publishScheduledReleaseVersionsNowEnabled!
    functionAppTimeZone: publisherConfig.functionAppTimeZone!
    prepareScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.prepareScheduledReleaseVersionsFunctionCronSchedule!
    publishScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.publishScheduledReleaseVersionsFunctionCronSchedule!
    adminAppUrl: 'https://admin.${environmentConfig.domain!}'
    publicAppUrl: 'https://${environmentConfig.domain!}'
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.publisher.databaseUserPassword)
    maintenanceIpRanges: environmentPipelineVariables.maintenanceIpRanges!
    blobDeleteRetentionDays: environmentConfig.blobDeleteRetentionDays!
    deployAlerts: true
    tagValues: tags
  }
  dependsOn: [
    // Publisher is dependent on Notifier's storage account being available
    // in order to reference its connection string secret in Key Vault.
    notifierModuleDeploy
  ]
}

module adminModuleDeploy '../admin/main.bicep' = {
  name: 'adminModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: adminConfig.appServiceSku!
    signalRAllowedOrigins: adminSiteAllowedOrigins
    signalRSku: adminConfig.signalRSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.admin.databaseUserPassword)
    existingProdAppSettings: adminProdAppSettings
    existingStagingSlotAppSettings: adminStagingSlotAppSettings
    tagValues: tags
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
    appServiceSku: contentApiConfig.appServiceSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.contentApi.databaseUserPassword)
    existingProdAppSettings: contentApiProdAppSettings
    existingStagingSlotAppSettings: contentApiStagingSlotAppSettings
    tagValues: tags
  }
}

module dataApiModuleDeploy '../data-api/main.bicep' = {
  name: 'dataApiModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: dataApiConfig.appServiceSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: keyVault.getSecret(resourceNames.keyVault.secrets.dataApi.databaseUserPassword)
    existingProdAppSettings: dataApiProdAppSettings
    existingStagingSlotAppSettings: dataApiStagingSlotAppSettings
    tagValues: tags
  }
}

module publicSiteModuleDeploy '../public-site/main.bicep' = {
  name: 'publicSiteModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: publicSiteConfig.appServiceSku!
    environmentName: environmentConfig.environmentName!
    googleAnalyticsTrackingId: publicSiteConfig.googleAnalyticsTrackingId!
    defaultCacheMaxAgeSeconds: publicSiteConfig.defaultCacheMaxAgeSeconds!
    publicApiPublicHostname: publicApiConfig.publicUrl!
    publicAppUrl: 'https://${environmentConfig.domain!}'
    contentApiPublicHostname: contentApiPublicHostname
    dataApiPublicHostname: dataApiPublicHostname
    dockerRegistryUrl: dockerRegistryUrl
    dockerPullUsername: keyVault.getSecret(resourceNames.keyVault.secrets.acr.dockerPullUsername)
    dockerPullPassword: keyVault.getSecret(resourceNames.keyVault.secrets.acr.dockerPullPassword)
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    publicAppBasicAuthEnabled: environmentConfig.basicAuthEnabled!
    publicAppBasicAuthUsername: keyVault.getSecret(resourceNames.keyVault.secrets.publicSite.basicAuthUsername)
    publicAppBasicAuthPassword: keyVault.getSecret(resourceNames.keyVault.secrets.publicSite.basicAuthPassword)
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    tagValues: tags
  }
}
