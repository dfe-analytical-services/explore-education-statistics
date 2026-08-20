using '../../templates/main.bicep'

param domain = 'test.explore-education-statistics.service.gov.uk'

param detailedErrors = true

param enableSwagger = true

param publicApiUrl = 'pp-api.education.gov.uk/statistics-test'
param publicApiDocsUrl = 'pp-api.education.gov.uk/statistics-test/docs'

param tableBuilderMaxTableCellsAllowed = 1000000

param prepareScheduledReleaseVersionsFunctionCronSchedule = '0 0 * * * *'
param publishScheduledReleaseVersionsFunctionCronSchedule = '0 30 * * * *'
