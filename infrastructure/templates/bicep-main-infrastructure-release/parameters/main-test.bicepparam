using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101t01'
  environmentName: 'Test'
  domain: 'test.explore-education-statistics.service.gov.uk'
  publicApiUrl: 'pp-api.education.gov.uk/statistics-test'
  publicApiDocsUrl: 'pp-api.education.gov.uk/statistics-test/docs'
  detailedErrors: true
  autoscaleAppServices: false
  enableSwagger: true
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
  basicAuthEnabled: true
}

param adminConfigParam = {
  enableThemeDeletion: true
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
  signalRSku: {
    name: 'Free_F1'
    capacity: 1
  }
}

param contentApiConfigParam = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}

param dataApiConfigParam = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-test'
}

param publicSiteConfigParam = {
  appServiceSku: {
    tier: 'Basic'
    name: 'B1'
  }
  googleAnalyticsTrackingId: 'G-ZQ5V7CBWMJ'
}

