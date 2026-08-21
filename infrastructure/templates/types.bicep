import { AppServicePlanSku } from 'common/components/app-service-plan/types.bicep'

@export()
type MemoryCacheConfig = {

  @description('The frequency of scans to evict expired entries from the in-memory cache.')
  expirationScanFrequencySeconds: int
  
  @description('Max size of in-memory cache in MBs.  This is an approximation based on the size of the cached objects in JSON notation.')
  maxCacheSizeMb: int
  
  @description('Override duration in seconds for all entities cached in memory')
  overridesDurationInSeconds: int?

  @description('Override cron expression for all entities cached in memory')
  overridesExpirySchedule: string?
}

@export()
type Tags = {
  
  @description('Tag Value - the Department name tag value e.g. Data Directorate')
  Department: string
  
  @description('Tag Value - the solution name that the component is a part of e.g. EDAP, LDS, EES')
  Solution: string
  
  @description('Tag Value - The name of the phase of the development lifecycle environment that the component will be used in e.g. Development / Test / Pre-Production / Production')
  Environment: string

  @description('Tag Value - the full name of the Azure subscription where this resource is located e.g. s101-datahub-development / s101-datahub-test / s101-datahub-production')
  Subscription: string

  @description('Tag Value - the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
  CostCentre: string
  
  @description('Tag Value - the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
  ServiceOwner: string
  
  @description('Tag Value - the date that the component was created using the YYYYMMDD format e.g. 20190417. Use of the utcNow function will automatically populate this entry at creation time. Note: This only works when forced as a default value.')
  DateProvisioned: string
  
  @description('Tag Value - the name of the user who created these resources in the SURNAME, Firstname format e.g. RULER, Paul')
  CreatedBy: string
  
  @description('Tag Value - the name of the repo that the deployment script for the component name be found. If the component is deployed manually, the value should be N/A')
  DeploymentRepo: string
  
  @description('Tag Value - the name of the main script (not the parameters file) used to deploy the component. If the component is deployed manually, the value should be N/A')
  DeploymentScript: string
}

@export()
type EnvironmentConfig = {

  @description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
  environmentIdentifier: string?

  @description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
  domain: string?

  @description('Public URL of the public API.')
  publicApiUrl: string?

  @description('Public URL of the public API documentation site.')
  publicApiDocsUrl: string?

  @description('Cron expression that defines when the PrepareScheduledReleaseVersions function runs in the Publisher Function App.')
  prepareScheduledReleaseVersionsFunctionCronSchedule: string?

  @description('Cron expression that defines when the PublishScheduledReleaseVersions function runs in the Publisher Function App')
  publishScheduledReleaseVersionsFunctionCronSchedule: string?

  @description('Maximum number of table cells that a table builder query could potentially render for a request to be valid.')
  tableBuilderMaxTableCellsAllowed: int?

  @description('Global configuration for memory caches.')
  memoryCacheConfig: MemoryCacheConfig?

  @description('Whether or not to enable autoscaling of App Services in this environment.')
  autoscaleAppServices: bool?

  @description('Whether or not to enable detailed error messages in this environment.')
  detailedErrors: bool?

  @description('Whether or not to enable Swagger API pages in this environment.')
  enableSwagger: bool?
}

@export()
type AdminConfig = {

  @description('Admin App Service SKU')
  appServiceSku: AppServicePlanSku?

  @description('Whether or not to enable theme deletion in this environment (for test teardown).')
  enableThemeDeletion: bool?

  @description('Whether or not to enable published Education In Numbers pages deletion in this environment.')
  enableEinPublishedPageDeletion: bool?

  @description('Pre-release start time as number of minutes before a release is scheduled to be published.')
  preReleaseMinutesBeforeStart: int?

  pipelineVariables: {

    @description('Client ID of the public API Container App app registration in Entra ID.')
    apiAppRegistrationClientId: string?

    @description('Client ID of the public API processor app registration in Entra ID.')
    publicDataProcessorAppRegistrationClientId: string?

    @description('Client ID of the Screener API Function App app registration in Entra ID.')
    screenerAppRegistrationClientId: string?
  }?
}

