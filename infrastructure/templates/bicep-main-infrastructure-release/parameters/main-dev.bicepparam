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
}

param publisherConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
}
