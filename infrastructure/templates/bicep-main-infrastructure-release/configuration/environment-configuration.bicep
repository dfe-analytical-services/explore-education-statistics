import { MemoryCacheConfig } from '../types.bicep'

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

  @description('Additional CORS allowed origins for the public.')
  additionalPublicAllowedOrigins: string[]?

  @description('Whether analytics is enabled.')
  analyticsEnabled: bool?

  @description('Enables Basic Auth on the public application, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance).')
  basicAuthEnabled: bool?
}

@export()
type EnvironmentPipelineVariables = {

  @description('Username protecting the public app, no requirement to be secret, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance).')
  publicAppBasicAuthUsername: string?
}

var defaultConfig = {
  enableSwagger: false
  detailedErrors: false
  autoscaleAppServices: true
  analyticsEnabled: true
  basicAuthEnabled: false
  memoryCacheConfig: {
    expirationScanFrequencySeconds: 60
    maxCacheSizeMb: 50
  }
  tableBuilderMaxTableCellsAllowed: 1000000
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 5 0 * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 9 * * *'
}

@export()
func mergeEnvironmentConfig(overridden EnvironmentConfig) EnvironmentConfig =>
  union(
    defaultConfig,
    overridden,
    union(defaultConfig.memoryCacheConfig, overridden.?memoryCacheConfig ?? {})
  )
