using '../data-api-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101t01'
  environmentName: 'Test'
  domain: 'test.explore-education-statistics.service.gov.uk'
  enableSwagger: true
  basicAuthEnabled: true
}
