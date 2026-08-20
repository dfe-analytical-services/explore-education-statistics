using '../../templates/main.bicep'

param domain = 'dev.explore-education-statistics.service.gov.uk'

param detailedErrors = true

param enableSwagger = true

param enableThemeDeletion = true
param enableEinPublishedPageDeletion = true

param publicApiUrl = 'pp-api.education.gov.uk/statistics-dev'
param publicApiDocsUrl = 'pp-api.education.gov.uk/statistics-dev/docs'

param prepareScheduledReleaseVersionsFunctionCronSchedule = '0 0 * * * *'
param publishScheduledReleaseVersionsFunctionCronSchedule = '0 30 * * * *'

param preReleaseMinutesBeforeStart = 1440

param memoryCacheConfig = {
  expirationScanFrequencySeconds: 60
  maxCacheSizeMb: 50
  overridesDurationInSeconds: 10
}
