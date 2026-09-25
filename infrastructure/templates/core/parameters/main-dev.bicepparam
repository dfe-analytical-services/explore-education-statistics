using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicSiteInternalServiceFqdn = 's101d01-ees-fde-eve8c8hmd6gxgqcr.a03.azurefd.net'

param publicApiApplicationGatewayFqdn = 'dev.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-dev'

param blobDeleteRetentionDays = 3

param contentDbConfig = {
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 50
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 2147483648
}

param statisticsDbConfig = {
  sku: {
    name: 'GP_Gen5'
    tier: 'GeneralPurpose'
    capacity: 2
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 1099511627776
}
