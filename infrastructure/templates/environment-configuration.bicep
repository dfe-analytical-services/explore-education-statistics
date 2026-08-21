import { MemoryCacheConfig } from 'types.bicep'

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

var defaultEnvironmentConfig = {
  enableSwagger: false
  detailedErrors: false
  autoscaleAppServices: true
  memoryCacheConfig: {
    expirationScanFrequencySeconds: 60
    maxCacheSizeMb: 50
  }
  tableBuilderMaxTableCellsAllowed: 1000000
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 5 0 * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 9 * * *'
}

@export()
func mergeEnvironmentConfig(overridden EnvironmentConfig) EnvironmentConfig => {
  enableSwagger: overridden.?enableSwagger ?? defaultEnvironmentConfig.?enableSwagger
  detailedErrors: overridden.?detailedErrors ?? defaultEnvironmentConfig.?detailedErrors
  autoscaleAppServices: overridden.?autoscaleAppServices ?? defaultEnvironmentConfig.?autoscaleAppServices
  memoryCacheConfig: {
    expirationScanFrequencySeconds: overridden.?memoryCacheConfig.?expirationScanFrequencySeconds ?? defaultEnvironmentConfig.memoryCacheConfig.expirationScanFrequencySeconds
    maxCacheSizeMb: overridden.?memoryCacheConfig.?maxCacheSizeMb ?? defaultEnvironmentConfig.memoryCacheConfig.maxCacheSizeMb
    overridesDurationInSeconds: overridden.?memoryCacheConfig.?overridesDurationInSeconds
    overridesExpirySchedule: overridden.?memoryCacheConfig.?overridesExpirySchedule
  }
  tableBuilderMaxTableCellsAllowed: overridden.?tableBuilderMaxTableCellsAllowed ?? defaultEnvironmentConfig.tableBuilderMaxTableCellsAllowed
  prepareScheduledReleaseVersionsFunctionCronSchedule: overridden.?prepareScheduledReleaseVersionsFunctionCronSchedule ?? defaultEnvironmentConfig.prepareScheduledReleaseVersionsFunctionCronSchedule
  publishScheduledReleaseVersionsFunctionCronSchedule: overridden.?publishScheduledReleaseVersionsFunctionCronSchedule ?? defaultEnvironmentConfig.publishScheduledReleaseVersionsFunctionCronSchedule
  domain: overridden.?domain
  environmentIdentifier: overridden.?environmentIdentifier
  publicApiUrl: overridden.?publicApiUrl
  publicApiDocsUrl: overridden.?publicApiDocsUrl
}
