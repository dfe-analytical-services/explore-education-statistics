using '../admin-appsettings.bicep'

param environmentIdentifier = 's101d01'
param environmentName = 'Development'
param domain = 'dev.explore-education-statistics.service.gov.uk'
param enableSwagger = true
param prepareScheduledReleaseVersionsFunctionCronSchedule = '0 0 * * * *'
param publishScheduledReleaseVersionsFunctionCronSchedule = '0 30 * * * *'
param memoryCacheConfig = {
  expirationScanFrequencySeconds: 60
  maxCacheSizeMb: 50
  overridesDurationInSeconds: 10
}
param tableBuilderMaxTableCellsAllowed = 25000

param preReleaseMinutesBeforeStart = 1440
param enableThemeDeletion = true
param enableEinPublishedPageDeletion = true

param publicApiUrl = 'pp-api.education.gov.uk/statistics-dev'
