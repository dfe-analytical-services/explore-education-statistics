using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param publicUrls = {
  contentApi: 'https://s101p02-as-ees-content.azurewebsites.net'
  publicSite: 'https://pre-production.explore-education-statistics.service.gov.uk'
  publicApi: 'https://pre-production.statistics.api.education.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
param postgreSqlGeoRedundantBackupEnabled = true

param recoveryVaultImmutable = true

param enableThemeDeletion = false
