using '../main.bicep'

// Environment Params
param environmentName = 'Test'

param publicUrls = {
  contentApi: 'https://content.test.explore-education-statistics.service.gov.uk'
  publicSite: 'https://test.explore-education-statistics.service.gov.uk'
  publicApi: 'https://test.statistics.api.education.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'

param enableThemeDeletion = false
