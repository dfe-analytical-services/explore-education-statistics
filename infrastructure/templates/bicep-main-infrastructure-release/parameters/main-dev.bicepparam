using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101d01'
  domain: 'dev.explore-education-statistics.service.gov.uk'
  detailedErrors: true
  autoscaleAppServices: false
  enableSwagger: true
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
  publicApiUrl: 'pp-api.education.gov.uk/statistics-dev'
  publicApiDocsUrl: 'pp-api.education.gov.uk/statistics-dev/docs'
  memoryCacheConfig: {
    expirationScanFrequencySeconds: 60
    maxCacheSizeMb: 50
    overridesDurationInSeconds: 10
  }
  tableBuilderMaxTableCellsAllowed: 25000
  additionalAdminAllowedOrigins: [
    'https://localhost:5021'
    'http://localhost:5021'
  ]
  additionalPublicAllowedOrigins: [
    'http://localhost:3000'
  ]
  basicAuthEnabled: true
}

param adminConfigParam = {
  preReleaseMinutesBeforeStart: 1440
  enableThemeDeletion: true
  enableEinPublishedPageDeletion: true
}
