using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param averagePublicSiteResponseTimeAlertThresholdMillis = 15000

param publicSiteInternalServiceFqdn = 's101p01-ees-fde-hzgvd4b5effuaua2.a02.azurefd.net'

param publicApiApplicationGatewayFqdn = 'statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://api.education.gov.uk/statistics'


param recoveryServicesVaultImmutable = true

param contentDbConfig = {
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    capacity: 4
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 6442450944
  minCapacity: '0.5'
}

param statisticsDbConfig = {
  sku: {
    name: 'GP_Gen5'
    tier: 'GeneralPurpose'
    capacity: 12
  }
  licenseType: 'BasePrice'
  maxSizeBytes: 3298534883328
}
