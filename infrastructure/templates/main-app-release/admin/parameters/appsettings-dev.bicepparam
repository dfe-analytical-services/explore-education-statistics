using '../admin-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101d01'
  environmentName: 'Development'
  domain: 'dev.explore-education-statistics.service.gov.uk'
  enableSwagger: true
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
  memoryCacheConfig: {
    expirationScanFrequencySeconds: 60
    maxCacheSizeMb: 50
    overridesDurationInSeconds: 10
  }
  tableBuilderMaxTableCellsAllowed: 25000
}

param adminConfigParam = {
  preReleaseMinutesBeforeStart: 1440
  enableThemeDeletion: true
  enableEinPublishedPageDeletion: true
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-dev'
}
