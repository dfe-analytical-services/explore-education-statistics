using '../../templates/main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101t01'
  domain: 'test.explore-education-statistics.service.gov.uk'
  publicApiUrl: 'pp-api.education.gov.uk/statistics-test'
  publicApiDocsUrl: 'pp-api.education.gov.uk/statistics-test/docs'
  detailedErrors: true
  autoscaleAppServices: false
  enableSwagger: true
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
}

param adminConfigParam = {
  enableThemeDeletion: true
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}
