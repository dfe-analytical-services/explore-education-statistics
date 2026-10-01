using '../admin-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p01'
  environmentName: 'Production'
  domain: 'explore-education-statistics.service.gov.uk'
}

param publicApiConfigParam = {
  publicUrl: 'api.education.gov.uk/statistics'
}
