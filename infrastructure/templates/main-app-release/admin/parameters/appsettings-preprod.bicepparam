using '../admin-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p02'
  environmentName: 'Pre-Production'
  domain: 'pre-production.explore-education-statistics.service.gov.uk'
}

param adminConfigParam = {
  enableThemeDeletion: true
}

param publicApiConfigParam = {
  publicUrl: 'pp-api.education.gov.uk/statistics-preprod'
}
