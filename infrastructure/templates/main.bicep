import { getResourceNames } from 'resource-names.bicep'
import { Tags } from 'types.bicep'
import { EnvironmentConfig, mergeEnvironmentConfig } from 'environment-configuration.bicep'
import { AdminConfig, AdminPipelineVariables, mergeAdminConfig } from 'admin-configuration.bicep'

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



//
// Admin-specific config.
//
param adminConfigParam AdminConfig = {}

// Merge default configuration with overridden configuration from params files.
var adminConfig = mergeAdminConfig(adminConfigParam)

// These values are all supplied specifically by pipeline variables.
param adminPipelineVariables AdminPipelineVariables = {}

@secure()
@description('''Admin database user's password.''')
param sqlAdminUserPassword string = ''



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

module adminModule 'admin/main-admin.bicep' = {
  name: 'adminModuleDeploy'
  params: {
    resourceNames: resourceNames
    adminSku: adminConfig.appServiceSku!
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
    sqlAdminUserPassword: sqlAdminUserPassword
    tagValues: tags
  }
}
