using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param publicUrls = {
  contentApi: 'https://content.explore-education-statistics.service.gov.uk'
  publicSite: 'https://explore-education-statistics.service.gov.uk'
  publicApi: 'https://statistics.api.education.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
