using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param publicUrls = {
  contentApi: 'https://content.explore-education-statistics.service.gov.uk'
  publicSite: 'https://explore-education-statistics.service.gov.uk'
  publicApi: 'https://api.education.gov.uk/statistics'
  publicApiAppGateway: 'https://statistics.api.education.gov.uk'
}

param publicApiContainerAppConfig = {
  cpuCores: 4
  memoryGis: 8
  minReplicas: 1
  maxReplicas: 100
  scaleAtConcurrentHttpRequests: 10
  workloadProfileName: 'Consumption'
}

// PostgreSQL Database Params
param postgreSqlServerConfig = {
  sku: {
    pricingTier: 'Burstable'
    compute: 'Standard_B2s'
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
    {
      name: 'max_connections'
      value: '100'
    }
  ]
  storage: {
    storageSizeGB: 32
    autoGrow: true
  }
}

param docsAppSku = 'Standard'

param recoveryVaultImmutable = true

param searchServiceIndexName = 'index-1'

param enableThemeDeletion = false

param enableReplacementOfPublicApiDataSets = true

param deployPsqlBackupVaultRoleAssignment = true

param deployPsqlBackupVaultRegistration = true
