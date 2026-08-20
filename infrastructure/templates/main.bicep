import { getResourceNames } from 'resource-names.bicep'
import { AppServicePlanSku } from 'common/components/app-service-plan/types.bicep'
import { MemoryCacheConfig } from 'types.bicep'

@description('Tag Value - Enter the Department name tag value e.g. Data Directorate')
param departmentName string

@description('Tag Value - The name of the phase of the development lifecycle environment that the component will be used in e.g. Development / Test / Pre-Production / Production')
param environmentName string

@description('Tag Value - Enter the full name of the Azure subscription where this resource is located e.g. s101-datahub-development / s101-datahub-test / s101-datahub-production')
param subscriptionName string

@description('Tag Value - Enter the solution name that the component is a part of e.g. EDAP, LDS, EES')
param solutionName string

@description('Tag Value - Enter the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
param costCentre string

@description('Tag Value - Enter the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
param serviceOwnerName string

@description('Tag Value - Enter the date that the component was created using the YYYYMMDD format e.g. 20190417. Use of the utcNow function will automatically populate this entry at creation time. Note: This only works when forced as a default value.')
param dateProvisioned string

@description('Tag Value - Enter the name of the user who created these resources in the SURNAME, Firstname format e.g. RULER, Paul')
param createdBy string

@description('Tag Value - Enter the name of the repo that the deployment script for the component name be found. If the component is deployed manually, the value should be N/A')
param deploymentRepo string

@description('Tag Value - Enter the name of the main script (not the parameters file) used to deploy the component. If the component is deployed manually, the value should be N/A')
param deploymentScript string

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
param apiAppRegistrationClientId string

@description('Client ID of the public API processor app registration in Entra ID.')
param publicDataProcessorAppRegistrationClientId string

@description('Client ID of the Screener API Function App app registration in Entra ID.')
param screenerAppRegistrationClientId string

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
param sqlAdminUserPassword string

var tagValues = {
  Department: departmentName
  Solution: solutionName
  Environment: environmentName
  Subscription: subscriptionName
  CostCentre: costCentre
  ServiceOwner: serviceOwnerName
  DateProvisioned: dateProvisioned
  CreatedBy: createdBy
  DeploymentRepo: deploymentRepo
  DeploymentScript: deploymentScript
}

var legacyResourcePrefix = subscriptionName
var newResourcePrefix = '${subscriptionName}-ees'
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
