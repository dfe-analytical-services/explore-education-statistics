using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

param publicUrls = {
  contentApi: 'https://s101p02-as-ees-content.azurewebsites.net'
  publicSite: 'https://pre-production.explore-education-statistics.service.gov.uk'
  publicApi: 'https://pre-production.statistics.api.education.gov.uk'
}

// PostgreSQL Database Params
param postgreSqlServerConfig = {
  sku: {
    pricingTier: 'Burstable'
    compute: 'Standard_B1ms'
  }
  server: {
    postgreSqlVersion: '16'
  }
  backups: {
    retentionDays: 7
    geoRedundantBackup: true
  }
  settings: [
    {
      name: 'max_prepared_transactions'
      value: '100'
    }
  ]
  storage: {
    storageSizeGB: 32
    autoGrow: true
  }
}

param recoveryVaultImmutable = true

param enableThemeDeletion = false
