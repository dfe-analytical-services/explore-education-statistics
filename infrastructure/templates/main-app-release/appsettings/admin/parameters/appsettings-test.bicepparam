using '../admin-appsettings.bicep'

param environmentIdentifier = 's101t01'
param environmentName = 'Test'
param domain = 'test.explore-education-statistics.service.gov.uk'
param enableSwagger = true
param prepareScheduledReleaseVersionsFunctionCronSchedule = '0 0 * * * *'
param publishScheduledReleaseVersionsFunctionCronSchedule = '0 30 * * * *'

param enableThemeDeletion = true

param publicApiUrl = 'pp-api.education.gov.uk/statistics-test'
