using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param publicUrls = {
  contentApi: 'https://s101p02-as-ees-content.azurewebsites.net'
  publicApp: 'https://pre-production.explore-education-statistics.service.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlSkuName = 'Standard_B1ms'
param postgreSqlStorageSizeGB = 32
param postgreSqlAutoGrowStatus = 'Disabled'
