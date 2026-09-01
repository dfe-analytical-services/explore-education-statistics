import { getResourceNames } from 'resource-names.bicep'
import { Tags } from 'types.bicep'
import { EnvironmentConfig, EnvironmentPipelineVariables, mergeEnvironmentConfig } from 'configuration/environment-configuration.bicep'
import { AdminConfig, AdminPipelineVariables, mergeAdminConfig } from 'configuration/admin-configuration.bicep'
import { DataApiConfig, mergeDataApiConfig } from 'configuration/data-api-configuration.bicep'

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
// Data API-specific config.
//
param dataApiConfigParam DataApiConfig = {}

// Merge default configuration with overridden configuration from params files.
var dataApiConfig = mergeDataApiConfig(dataApiConfigParam)



//
// Secret pipeline variables (required to be top-level params).
//

@secure()
@description('''Data API database user's password for Azure SQL databases.''')
param dataApiAzureSqlPassword string = ''

@secure()
@description('''Admin database user's password for Azure SQL databases.''')
param adminAzureSqlPassword string = ''

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
  'https://${resourceNames.publicSite.appService.appServiceName}.azurewebsites.net'
  'https://${reference(afdEndpointResourceId, '2025-06-01').hostName}'
]

var publicSiteAllowedOrigins = union(basePublicAllowedOrigins, environmentConfig.?additionalPublicAllowedOrigins ?? [])

module adminModule '../admin/main.bicep' = {
  name: 'adminModuleDeploy'
  params: {
    resourceNames: resourceNames
    appServiceSku: adminConfig.appServiceSku!
    adminHostname: 'admin.${environmentConfig.domain!}'
    publicAppUrl: 'https://${environmentConfig.domain!}'
    autoscaleAppServices: environmentConfig.autoscaleAppServices!
    deployAlerts: true
    detailedErrors: environmentConfig.detailedErrors!
    enableSwagger: environmentConfig.enableSwagger!
    enableThemeDeletion: adminConfig.enableThemeDeletion!
    enableEinPublishedPageDeletion: adminConfig.enableEinPublishedPageDeletion!
    apiAppRegistrationClientId: adminPipelineVariables.apiAppRegistrationClientId!
    publicDataProcessorAppRegistrationClientId: adminPipelineVariables.publicDataProcessorAppRegistrationClientId!
    screenerAppRegistrationClientId: adminPipelineVariables.screenerAppRegistrationClientId!
    publicApiUrl: environmentConfig.publicApiUrl!
    publicApiDocsUrl: environmentConfig.publicApiDocsUrl!
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
