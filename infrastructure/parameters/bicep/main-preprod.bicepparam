using '../../templates/main.bicep'

param domain = 'pre-production.explore-education-statistics.service.gov.uk'

param autoscaleAppServices = true

param enableThemeDeletion = true

param publicApiUrl = 'pp-api.education.gov.uk/statistics-preprod'
param publicApiDocsUrl = 'pp-api.education.gov.uk/statistics-preprod/docs'

param tableBuilderMaxTableCellsAllowed = 1000000
