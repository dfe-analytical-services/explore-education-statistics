using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param publicUrls = {
  contentApi: 'https://content.explore-education-statistics.service.gov.uk'
  publicApp: 'https://explore-education-statistics.service.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
