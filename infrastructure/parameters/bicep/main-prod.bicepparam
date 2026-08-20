using '../../templates/main.bicep'

param domain = 'explore-education-statistics.service.gov.uk'

param autoscaleAppServices = true

param publicApiUrl = 'api.education.gov.uk/statistics'
param publicApiDocsUrl = 'api.education.gov.uk/statistics/docs'

param tableBuilderMaxTableCellsAllowed = 1000000
