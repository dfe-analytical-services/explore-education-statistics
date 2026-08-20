import { getResourceNames } from 'resource-names.bicep'
import { AppServicePlanSku } from 'common/components/app-service-plan/types.bicep'
import { MemoryCacheConfig, Tags } from 'types.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string = ''

@description('Tag Value - Enter the Department name tag value e.g. Data Directorate')
param tagValues Tags = {
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

@description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
param domain string

@description('Admin App Service SKU')
param adminSku AppServicePlanSku = {
  tier: 'Premium'
  name: 'P1V2'
} 

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool = false

@description('Whether or not to enable detailed error messages in this environment.')
param detailedErrors bool = false

@description('Whether or not to enable Swagger API pages in this environment.')
param enableSwagger bool = false

@description('Whether or not to enable theme deletion in this environment (for test teardown).')
param enableThemeDeletion bool = false

@description('Whether or not to enable published Education In Numbers pages deletion in this environment.')
param enableEinPublishedPageDeletion bool = false

@description('Client ID of the public API Container App app registration in Entra ID.')
param apiAppRegistrationClientId string = ''

@description('Client ID of the public API processor app registration in Entra ID.')
param publicDataProcessorAppRegistrationClientId string = ''

@description('Client ID of the Screener API Function App app registration in Entra ID.')
param screenerAppRegistrationClientId string = ''

@description('Public URL of the public API.')
param publicApiUrl string

@description('Public URL of the public API documentation site.')
param publicApiDocsUrl string

@description('Pre-release start time as number of minutes before a release is scheduled to be published.')
param preReleaseMinutesBeforeStart int = 870

@description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs in the Publisher Function App.')
param prepareScheduledReleaseVersionsFunctionCronSchedule string = '0 5 0 * * *'

@description('Cron expression that defines when the PublishScheduledReleaseVersions function runs in the Publisher Function App')
param publishScheduledReleaseVersionsFunctionCronSchedule string = '0 30 9 * * *'

@description('Maximum number of table cells that a table builder query could potentially render for a request to be valid.')
param tableBuilderMaxTableCellsAllowed int = 25000

@description('Global configuration for memory caches.')
param memoryCacheConfig MemoryCacheConfig = {
  expirationScanFrequencySeconds: 60
  maxCacheSizeMb: 50
}

@secure()
@description('''Admin database user's password.''')
param sqlAdminUserPassword string = ''

var legacyResourcePrefix = environmentIdentifier
var newResourcePrefix = '${environmentIdentifier}-ees'
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
    adminSku: adminSku
    adminHostname: 'admin.${domain}'
    publicAppUrl: 'https://${domain}'
    autoscaleAppServices: autoscaleAppServices
    deployAlerts: true
    detailedErrors: detailedErrors
    enableSwagger: enableSwagger
    enableThemeDeletion: enableThemeDeletion
    enableEinPublishedPageDeletion: enableEinPublishedPageDeletion
    apiAppRegistrationClientId: apiAppRegistrationClientId
    publicDataProcessorAppRegistrationClientId: publicDataProcessorAppRegistrationClientId
    screenerAppRegistrationClientId: screenerAppRegistrationClientId
    publicApiUrl: publicApiUrl
    publicApiDocsUrl: publicApiDocsUrl
    prepareScheduledReleaseVersionsFunctionCronSchedule: prepareScheduledReleaseVersionsFunctionCronSchedule
    publishScheduledReleaseVersionsFunctionCronSchedule: publishScheduledReleaseVersionsFunctionCronSchedule
    preReleaseMinutesBeforeStart: preReleaseMinutesBeforeStart
    tableBuilderMaxTableCellsAllowed: tableBuilderMaxTableCellsAllowed
    minTlsVersion: minTlsVersion
    memoryCacheConfig: memoryCacheConfig
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    sqlAdminUserPassword: sqlAdminUserPassword
    tagValues: tagValues
  }
}
