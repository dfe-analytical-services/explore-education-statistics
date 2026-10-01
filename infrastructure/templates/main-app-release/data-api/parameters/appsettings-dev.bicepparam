using '../data-api-bicep-config.bicep'

param environmentConfigParam = {
  environmentIdentifier: 's101d01'
  environmentName: 'Development'
  domain: 'dev.explore-education-statistics.service.gov.uk'
  enableSwagger: true
  basicAuthEnabled: true
  tableBuilderMaxTableCellsAllowed: 25000
}
