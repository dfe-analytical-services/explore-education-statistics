using '../admin-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101t01'
  environmentName: 'Test'
  domain: 'test.explore-education-statistics.service.gov.uk'
  enableSwagger: true
  prepareScheduledReleaseVersionsFunctionCronSchedule: '0 0 * * * *'
  publishScheduledReleaseVersionsFunctionCronSchedule: '0 30 * * * *'
}

param adminConfigParam = {
  enableThemeDeletion: true
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-test'
}
