import { IpRange } from '../../common/types.bicep'

@export()
type EnvironmentConfig = {

  @description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
  environmentIdentifier: string?

  @description('Name of this environment e.g. Development, Test.')
  environmentName: string?

  @description('The main domain of this environment e.g. dev.explore-education-statistics.service.gov.uk.')
  domain: string?

  @description('Whether or not to enable autoscaling of App Services in this environment.')
  autoscaleAppServices: bool?

  @description('Whether or not to enable detailed error messages in this environment.')
  detailedErrors: bool?

  @description('Additional CORS allowed origins for the Admin site.')
  additionalAdminAllowedOrigins: string[]?

  @description('Additional CORS allowed origins for the public site.')
  additionalPublicAllowedOrigins: string[]?

  @description('Whether analytics is enabled.')
  analyticsEnabled: bool?

  @description('Number of days to retain blobs after delete.')
  blobDeleteRetentionDays: int?
}

@export()
type EnvironmentPipelineVariables = {

  @description('Provides access to resources for specific IP address ranges used for service maintenance.')
  maintenanceIpRanges: IpRange[]?
}

var defaultConfig = {
  detailedErrors: false
  autoscaleAppServices: true
  analyticsEnabled: true
  blobDeleteRetentionDays: 90
}

@export()
func mergeEnvironmentConfig(overridden EnvironmentConfig) EnvironmentConfig =>
  union(defaultConfig, overridden)
