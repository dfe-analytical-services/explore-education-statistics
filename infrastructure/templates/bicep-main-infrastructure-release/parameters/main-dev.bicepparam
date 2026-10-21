using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101d01'
  environmentName: 'Development'
  domain: 'dev.explore-education-statistics.service.gov.uk'
  detailedErrors: true
  autoscaleAppServices: false
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
  additionalAdminAllowedOrigins: [
    'https://localhost:5021'
    'http://localhost:5021'
  ]
  additionalPublicAllowedOrigins: [
    'http://localhost:3000'
  ]
  basicAuthEnabled: true
  blobDeleteRetentionDays: 3
}

param notifierConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
  suppressExceptionsForTeamOnlyApiKeyErrors: true
}

param publisherConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
  prepareScheduledReleaseVersionsNowEnabled: true
  publishScheduledReleaseVersionsNowEnabled: true
  frontDoorCachePurgeEnabled: true
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-dev'
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
  googleAnalyticsTrackingId: 'G-GRPHH2FN0L'
  defaultCacheMaxAgeSeconds: 10
}
