using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param publicUrls = {
  contentApi: 'https://content.test.explore-education-statistics.service.gov.uk'
  publicSite: 'https://test.explore-education-statistics.service.gov.uk'
  publicApi: 'https://pp-api.education.gov.uk/statistics-test'
  publicApiAppGateway: 'https://test.statistics.api.education.gov.uk'
}

param searchServiceIndexName = 'index-1'

param enableThemeDeletion = false

param deployPsqlBackupVaultRoleAssignment = true

param deployPsqlBackupVaultRegistration = true
