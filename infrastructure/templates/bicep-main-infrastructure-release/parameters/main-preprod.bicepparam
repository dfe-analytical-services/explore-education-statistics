using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p02'
  domain: 'pre-production.explore-education-statistics.service.gov.uk'
  publicApiUrl: 'pp-api.education.gov.uk/statistics-preprod'
  publicApiDocsUrl: 'pp-api.education.gov.uk/statistics-preprod/docs'
}

param adminConfigParam = {
  enableThemeDeletion: true
}
