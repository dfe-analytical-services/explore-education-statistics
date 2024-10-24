using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicUrls = {
  contentApi: 'https://content.dev.explore-education-statistics.service.gov.uk'
  publicSite: 'https://dev.explore-education-statistics.service.gov.uk'
  publicApi: 'https://dev.statistics.api.education.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
