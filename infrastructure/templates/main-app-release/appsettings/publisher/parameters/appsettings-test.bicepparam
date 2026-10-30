using '../publisher-appsettings.bicep'

param environmentName = 'Test'
param domain = 'test.explore-education-statistics.service.gov.uk'
param prepareScheduledReleaseVersionsNowEnabled = true
param publishScheduledReleaseVersionsNowEnabled = true
param prepareScheduledReleaseVersionsFunctionCronSchedule = '0 0 * * * *'
param publishScheduledReleaseVersionsFunctionCronSchedule = '0 30 * * * *'
