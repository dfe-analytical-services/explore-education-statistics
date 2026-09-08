import { getResourceNames } from 'resource-names.bicep'
import { Tags } from 'types.bicep'
import { EnvironmentConfig, EnvironmentPipelineVariables, mergeEnvironmentConfig } from 'configuration/environment-configuration.bicep'
import { AdminConfig, AdminPipelineVariables, mergeAdminConfig } from 'configuration/admin-configuration.bicep'
import { ContentApiConfig, mergeContentApiConfig } from 'configuration/content-api-configuration.bicep'
import { DataApiConfig, mergeDataApiConfig } from 'configuration/data-api-configuration.bicep'
import { PublicApiConfig, mergePublicApiConfig } from 'configuration/public-api-configuration.bicep'
import { PublicSiteConfig, mergePublicSiteConfig } from 'configuration/public-site-configuration.bicep'

//
// Tagging config.
//

@description('Tags for tagging resources created in Azure. These are all fed in from pipeline variables.')
param tags Tags = {
  Department: ''
  Solution: ''
  Environment: ''
  Subscription: ''
  CostCentre: ''
  ServiceOwner: ''
  DateProvisioned: ''
  CreatedBy: ''
  DeploymentRepo: ''
  DeploymentScript: ''
}



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

// These values are all supplied specifically by pipeline variables.
param adminPipelineVariables AdminPipelineVariables = {}



//
// Content API-specific config.
//
param contentApiConfigParam ContentApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var contentApiConfig = mergeContentApiConfig(contentApiConfigParam)



//
// Data API-specific config.
//
param dataApiConfigParam DataApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var dataApiConfig = mergeDataApiConfig(dataApiConfigParam)



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
// Secret pipeline variables (required to be top-level params).
//

@secure()
@description('''Admin database user's password for Azure SQL databases.''')
param adminAzureSqlPassword string = ''

@secure()
@description('''Content API database user's password for Azure SQL databases.''')
param contentApiAzureSqlPassword string = ''

@secure()
@description('''Data API database user's password for Azure SQL databases.''')
param dataApiAzureSqlPassword string = ''

@secure()
@description('Password protecting the public app, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance).')
param publicAppBasicAuthPassword string = ''



//
// Resource provisioning.
//

var legacyResourcePrefix = environmentConfig.environmentIdentifier!
var newResourcePrefix = '${environmentConfig.environmentIdentifier!}-ees'
var publicApiResourcePrefix = '${newResourcePrefix}-papi'
var screenerResourcePrefix = '${newResourcePrefix}-sapi'

var resourceNames = getResourceNames(
  legacyResourcePrefix,
  publicApiResourcePrefix,
  screenerResourcePrefix,
  newResourcePrefix
)

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

var contentApiPublicHostname = '${environmentConfig.environmentName! == 'Pre-Production' ? 'cont' : 'content'}.${environmentConfig.domain!}'
var dataApiPublicHostname = 'data.${environmentConfig.domain!}'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.keyVault.keyVault
}

var dockerRegistryUrl = 'https://${resourceNames.acr.serverName}${environment().suffixes.acrLoginServer}'

module adminModule '../admin/main.bicep' = {
  name: 'adminModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: adminConfig.appServiceSku!
    adminHostname: 'admin.${environmentConfig.domain!}'
    publicAppUrl: 'https://${environmentConfig.domain!}'
    signalRAllowedOrigins: adminSiteAllowedOrigins
    signalRSku: adminConfig.signalRSku!
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    enableSwagger: environmentConfig.enableSwagger!
    enableThemeDeletion: adminConfig.enableThemeDeletion!
    enableEinPublishedPageDeletion: adminConfig.enableEinPublishedPageDeletion!
    apiAppRegistrationClientId: adminPipelineVariables.apiAppRegistrationClientId!
    publicDataProcessorAppRegistrationClientId: adminPipelineVariables.publicDataProcessorAppRegistrationClientId!
    screenerAppRegistrationClientId: adminPipelineVariables.screenerAppRegistrationClientId!
    publicApiUrl: publicApiConfig.publicUrl!
    publicApiDocsUrl: '${publicApiConfig.publicUrl!}/docs'
    prepareScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.prepareScheduledReleaseVersionsFunctionCronSchedule!
    publishScheduledReleaseVersionsFunctionCronSchedule: environmentConfig.publishScheduledReleaseVersionsFunctionCronSchedule!
    preReleaseMinutesBeforeStart: adminConfig.preReleaseMinutesBeforeStart!
    tableBuilderMaxTableCellsAllowed: environmentConfig.tableBuilderMaxTableCellsAllowed!
    minTlsVersion: minTlsVersion
    memoryCacheConfig: environmentConfig.memoryCacheConfig!
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: adminAzureSqlPassword
    tagValues: tags
  }
}

module contentApiModuleDeploy '../content-api/main.bicep' = {
  name: 'contentApiModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: contentApiConfig.appServiceSku!
    publicAppUrl: 'https://${environmentConfig.domain!}'
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    enableSwagger: environmentConfig.enableSwagger!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: contentApiAzureSqlPassword
    tagValues: tags
  }
}

module dataApiModuleDeploy '../data-api/main.bicep' = {
  name: 'dataApiModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: dataApiConfig.appServiceSku!
    publicAppUrl: 'https://${environmentConfig.domain!}'
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    allowedOrigins: publicSiteAllowedOrigins
    analyticsEnabled: environmentConfig.analyticsEnabled!
    publicAppBasicAuth: environmentConfig.basicAuthEnabled!
    publicAppBasicAuthUsername: environmentPipelineVariables.publicAppBasicAuthUsername!
    publicAppBasicAuthPassword: publicAppBasicAuthPassword
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    enableSwagger: environmentConfig.enableSwagger!
    tableBuilderMaxTableCellsAllowed: environmentConfig.tableBuilderMaxTableCellsAllowed!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    databaseUserPassword: dataApiAzureSqlPassword
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
    publicAppBasicAuth: environmentConfig.basicAuthEnabled!
    publicAppBasicAuthUsername: environmentPipelineVariables.publicAppBasicAuthUsername!
    publicAppBasicAuthPassword: publicAppBasicAuthPassword
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    minTlsVersion: minTlsVersion
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    tagValues: tags
  }
}
