using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicUrls = {
  contentApi: 'https://content.dev.explore-education-statistics.service.gov.uk'
  publicApp: 'https://dev.explore-education-statistics.service.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
