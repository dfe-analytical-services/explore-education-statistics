using '../main.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101p01'
  domain: 'explore-education-statistics.service.gov.uk'
  publicApiUrl: 'api.education.gov.uk/statistics'
  publicApiDocsUrl: 'api.education.gov.uk/statistics/docs'
}
